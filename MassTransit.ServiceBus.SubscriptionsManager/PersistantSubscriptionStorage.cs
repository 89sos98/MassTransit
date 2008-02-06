/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.
namespace MassTransit.ServiceBus.SubscriptionsManager
{
    using System;
    using System.Collections.Generic;
    using log4net;
    using NHibernate;
    using NHibernate.Expression;
    using Subscriptions;

    public class PersistantSubscriptionStorage : ISubscriptionStorage
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (SubscriptionService));
        private readonly ISessionFactory _factory;
        private readonly object _locker = new object();

        public PersistantSubscriptionStorage(ISessionFactory factory)
        {
            _factory = factory;
        }

        #region ISubscriptionStorage Members

        public void Add(string messageName, Uri endpoint)
        {
            try
            {
                using (ISession sess = _factory.OpenSession())
                using (ITransaction tr = sess.BeginTransaction())
                {
                    ICriteria crit = sess.CreateCriteria(typeof (StoredSubscription));

                    crit.Add(Expression.Eq("Address", endpoint.ToString()))
                        .Add(Expression.Eq("Message", messageName));

                    StoredSubscription obj = crit.UniqueResult<StoredSubscription>();

                    if (obj == null)
                    {
                        obj = new StoredSubscription(endpoint.ToString(), messageName);
                        sess.Save(obj);
                    }
                    else
                    {
                        obj.IsActive = true;
                        sess.Update(obj);
                    }

                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Error adding message {0} for address {1} to the repository", messageName, endpoint), ex);
                throw;
            }
        }

        public void Remove(string messageName, Uri endpoint)
        {
            using (ISession sess = _factory.OpenSession())
            using (ITransaction tr = sess.BeginTransaction())
            {
                ICriteria crit = sess.CreateCriteria(typeof (StoredSubscription));

                crit.Add(Expression.Eq("Address", endpoint.ToString()))
                    .Add(Expression.Eq("Message", messageName));

                StoredSubscription obj = crit.UniqueResult<StoredSubscription>();
                if (obj != null)
                {
                    obj.IsActive = false;

                    sess.Update(obj);
                }

                tr.Commit();
            }
        }

        public event EventHandler<SubscriptionChangedEventArgs> SubscriptionChanged;

        public IList<Subscription> List()
        {
            using (ISession sess = _factory.OpenSession())
            {
                ICriteria crit = sess.CreateCriteria(typeof (StoredSubscription))
                    .Add(Expression.Eq("IsActive", true));

                return SubscriptionMapper.MapFrom(crit.List<StoredSubscription>());
            }
        }

        public IList<Subscription> List(string messageName)
        {
            using (ISession sess = _factory.OpenSession())
            {
                ICriteria crit = sess.CreateCriteria(typeof (StoredSubscription))
                    .Add(Expression.Eq("Message", messageName))
                    .Add(Expression.Eq("IsActive", true));

                return SubscriptionMapper.MapFrom(crit.List<StoredSubscription>());
            }
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        #endregion
    }
}