// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Saga
{
	using System;
	using System.Linq;
	using System.Linq.Expressions;
	using Magnum;
	using Magnum.Reflection;
	using Util;

	public class InitiatingSagaPolicy<TSaga, TMessage> :
		ISagaPolicy<TSaga, TMessage>
		where TSaga : class, ISaga
	{
		readonly Func<TSaga, bool> _shouldBeRemoved;
		readonly bool _useMessageIdForSagaId;


		public InitiatingSagaPolicy(Expression<Func<TSaga, bool>> shouldBeRemoved)
		{
			_useMessageIdForSagaId = typeof (TMessage).GetInterfaces().Where(x => x == typeof (CorrelatedBy<Guid>)).Any();
			_shouldBeRemoved = shouldBeRemoved.Compile();
		}

		public bool CanCreateInstance(IConsumeContext<TMessage> context)
		{
			return true;
		}

		public TSaga CreateInstance(IConsumeContext<TMessage> context, Guid sagaId)
		{
			return FastActivator<TSaga>.Create(sagaId);
		}

		public Guid GetNewSagaId(IConsumeContext<TMessage> context)
		{
			Guid sagaId;
			if (!UseMessageIdForSaga(context.Message, out sagaId))
			{
				if (!GenerateNewIdForSaga(out sagaId))
					throw new InvalidOperationException("Could not generate id for new saga " + typeof (TSaga).Name);
			}

			return sagaId;
		}

		public bool CanUseExistingInstance(IConsumeContext<TMessage> context)
		{
			return false;
		}

		public bool CanRemoveInstance(TSaga instance)
		{
			return _shouldBeRemoved(instance);
		}

		bool UseMessageIdForSaga(TMessage message, out Guid sagaId)
		{
			if (_useMessageIdForSagaId)
			{
				var correlator = message.TranslateTo<CorrelatedBy<Guid>>();

				sagaId = correlator.CorrelationId;

				return true;
			}

			sagaId = CombGuid.Generate();
			return true;
		}

		static bool GenerateNewIdForSaga(out Guid sagaId)
		{
			sagaId = CombGuid.Generate();
			return true;
		}
	}
}