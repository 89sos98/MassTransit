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

using System;
using System.Collections.Generic;
using System.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;
using MassTransit.ServiceBus.Subscriptions.Messages;

namespace MassTransit.ServiceBus.Subscriptions
{
    public class MsmqSubscriptionStorage :
        ISubscriptionStorage
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (MsmqSubscriptionStorage));

        private BinaryFormatter _formatter;
        private Cursor _peekCursor;
        private MessageQueue _storageQueue;
        private readonly IMessageQueueEndpoint _storageEndpoint;
        private readonly ISubscriptionStorage _subscriptionCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageEndpoint">the name of the queue that stores all of the subscriptions</param>
        /// <param name="subscriptionCache">in memory cache</param>
        public MsmqSubscriptionStorage(IMessageQueueEndpoint storageEndpoint, ISubscriptionStorage subscriptionCache)
        {
			_storageEndpoint = storageEndpoint;
            _subscriptionCache = subscriptionCache;
			_storageQueue = storageEndpoint.Open(QueueAccessMode.SendAndReceive);

            //TODO: should there be a bus instance here so we can subscribe to messages and send messages?

            Initialize();
        }


        public event EventHandler<SubscriptionChangedEventArgs> SubscriptionChanged;

        private void Initialize()
        {
            _formatter = new BinaryFormatter();

            _peekCursor = _storageQueue.CreateCursor();

            _storageQueue.BeginPeek(TimeSpan.FromHours(24), _peekCursor, PeekAction.Current, this, QueuePeekCompleted);
        }


        private void QueuePeekCompleted(IAsyncResult asyncResult)
        {
            if (_storageQueue == null) // handles if our queue has been closed/disposed but we are being notified afterwards
                return;

            Message msg = _storageQueue.EndPeek(asyncResult);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Subscription Update Received: Id {0}", msg.Id);

            IMessage[] messages = _formatter.Deserialize(msg.BodyStream) as IMessage[];
            if (messages != null)
            {
                foreach (SubscriptionChange changeMsg in messages)
                {
                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Subscription Subscribe: {0} Message Type: {1} Mode: {2}", msg.ResponseQueue.Path, changeMsg.Subscription.MessageName, changeMsg.ChangeType.ToString());

                    if (changeMsg.ChangeType == SubscriptionChangeType.Add) //would there ever be anything but?
                    {
                        _subscriptionCache.Add(changeMsg.Subscription.MessageName, changeMsg.Subscription.Address);
                    }
                    else
                    {
                        _subscriptionCache.Remove(changeMsg.Subscription.MessageName, changeMsg.Subscription.Address);
                        _storageQueue.ReceiveById(msg.Id); //rip out of the queue
                    }
                }
            }

            _storageQueue.BeginPeek(TimeSpan.FromHours(24), _peekCursor, PeekAction.Next, this, QueuePeekCompleted);
        }

        public IList<Subscription> List(string messageName)
        {
            return _subscriptionCache.List(messageName);
        }

        public IList<Subscription> List()
        {
            return _subscriptionCache.List();
        }

        public void Add(string messageName, Uri endpoint)
        {
            SubscriptionChange subscriptionChange =
                new SubscriptionChange(messageName, endpoint,
                                        SubscriptionChangeType.Add);
            
            _subscriptionCache.Add(messageName, endpoint);
            InternalSend(subscriptionChange);
            OnChange(subscriptionChange);
            

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Adding Subscription to {0} for {1}", messageName, endpoint);
        }


        public void Remove( string messageName, Uri endpoint)
        {
            _subscriptionCache.Remove(messageName, endpoint);

            SubscriptionChange subscriptionChange =
                new SubscriptionChange(messageName, endpoint,
                                        SubscriptionChangeType.Remove);

            InternalSend(subscriptionChange);
            OnChange(subscriptionChange);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Removing Subscription to {0} for {1}", messageName, endpoint);
        }

        protected void OnChange(SubscriptionChange change)
        {
            EventHandler<SubscriptionChangedEventArgs> handler = SubscriptionChanged;
            if(handler != null)
            {
                handler(this, new SubscriptionChangedEventArgs(change));
            }
        }

        private void InternalSend(IMessage message)
        {
            Message msg = new Message();

            msg.ResponseQueue = new MessageQueue(_storageEndpoint.QueuePath);
            msg.Recoverable = true;

            _formatter.Serialize(msg.BodyStream, new IMessage[] {message});

            _storageQueue.Send(msg);
        }

        public void Dispose()
        {
            _storageQueue.Close();
            _storageQueue.Dispose();
            _storageQueue = null;

            _subscriptionCache.Dispose();
        }
    }
}