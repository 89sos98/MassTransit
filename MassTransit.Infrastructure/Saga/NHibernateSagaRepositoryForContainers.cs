// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Infrastructure.Saga
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using log4net;
	using MassTransit.Saga;
	using NHibernate;
	using NHibernate.Linq;

	public class NHibernateSagaRepositoryForContainers<T> :
		ISagaRepository<T>
		where T : class, ISaga
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (NHibernateSagaRepositoryForContainers<T>).ToFriendlyName());
		private volatile bool _disposed;
		private ISessionFactory _sessionFactory;

		public NHibernateSagaRepositoryForContainers(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public IEnumerable<Action<V>> Find<V>(Expression<Func<T, bool>> expression, Action<T, V> action)
		{
			using (var session = _sessionFactory.OpenSession())
			{
				List<T> existingSagas = session.Linq<T>().Where(expression).ToList();

				foreach (T saga in existingSagas)
				{
					if (_log.IsDebugEnabled)
						_log.DebugFormat("Found saga [{0}] - {1}", typeof (T).ToFriendlyName(), saga.CorrelationId);

					var sagaInstance = saga;
					yield return x => action(sagaInstance, x);

					session.Update(saga);
				}
				session.Flush();
			}
		}

		public IEnumerable<Action> Find(Expression<Func<T, bool>> expression, Action<T> action)
		{
			foreach (var item in Find<int>(expression, (s, m) => action(s)))
			{
				Action<int> actionItem = item;

				yield return () => actionItem(0);
			}
		}

		public IEnumerable<T> Where(Expression<Func<T, bool>> filter)
		{
			using (var session = _sessionFactory.OpenSession())
			{
				return session.Linq<T>().Where(filter).ToList();
			}
		}

		public IEnumerable<Action<V>> Create<V>(Guid sagaId, Action<T, V> action)
		{
			T saga = (T) Activator.CreateInstance(typeof (T), sagaId);

			if (_log.IsDebugEnabled)
				_log.DebugFormat("Created saga [{0}] - {1}", typeof (T).ToFriendlyName(), sagaId);

			yield return x => action(saga, x);

			using (var session = _sessionFactory.OpenSession())
			{
				session.Save(saga);
				session.Flush();
			}
		}

		public void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_sessionFactory = null;
			}
			_disposed = true;
		}

		~NHibernateSagaRepositoryForContainers()
		{
			Dispose(false);
		}
	}
}