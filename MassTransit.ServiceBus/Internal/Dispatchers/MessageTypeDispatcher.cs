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
namespace MassTransit.ServiceBus.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

	/// <summary>
    /// Manages and dispatches messages to correlated message consumers
    /// </summary>
    public class MessageTypeDispatcher :
        IMessageTypeDispatcher
    {
        private readonly Dictionary<Type, IMessageDispatcher> _messageDispatchers = new Dictionary<Type, IMessageDispatcher>();
        private readonly ReaderWriterLock _messageLock = new ReaderWriterLock();

        public bool Accept(object message)
        {
            IMessageDispatcher dispatcher;
            bool found = GetMessageDispatcher(message, out dispatcher);
            if (found)
                return dispatcher.Accept(message);

            return false;
        }

        public void Consume(object message)
        {
            IMessageDispatcher dispatcher;
            bool found = GetMessageDispatcher(message, out dispatcher);

            if (found)
                dispatcher.Consume(message);
        }

        public void Dispose()
        {
            foreach (IMessageDispatcher messageDispatcher in _messageDispatchers.Values)
            {
                messageDispatcher.Dispose();
            }
            _messageDispatchers.Clear();
        }

        public void Attach<T>(Consumes<T>.All consumer) where T : class
        {
            Produces<T> dispatcher = GetMessageDispatcher<T>();

            dispatcher.Attach(consumer);
        }

        public void Detach<T>(Consumes<T>.All consumer) where T : class
        {
            Produces<T> dispatcher = GetMessageDispatcher<T>();

            dispatcher.Detach(consumer);
        }

        public IMessageDispatcher<T> GetMessageDispatcher<T>() where T : class
        {
            Type messageType = typeof (T);

            return (IMessageDispatcher<T>)GetMessageDispatcher(messageType);
        }

        public IMessageDispatcher GetMessageDispatcher(Type messageType) 
        {
			_messageLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                IMessageDispatcher consumer;
            	if (_messageDispatchers.TryGetValue(messageType, out consumer))
            		return consumer;

                LockCookie cookie = _messageLock.UpgradeToWriterLock(Timeout.Infinite);
				try
				{
					if (_messageDispatchers.TryGetValue(messageType, out consumer))
						return consumer;

					Type dispatcherType = typeof(MessageDispatcher<>).MakeGenericType(messageType);

					consumer = (IMessageDispatcher) Activator.CreateInstance(dispatcherType);

					_messageDispatchers.Add(messageType, consumer);

					return consumer;
				}
				finally
				{
					_messageLock.DowngradeFromWriterLock(ref cookie);
				}
			}
			finally
            {
            	_messageLock.ReleaseReaderLock();
            }
        }

        public T GetDispatcher<T>() where T : class
        {
            return GetDispatcher<T>(typeof (T));
        }

        public T GetDispatcher<T>(Type type) where T : class
        {
            Type typeOfT = typeof(T);

            if (!typeOfT.IsGenericType)
                return null;

            if (typeOfT.GetGenericTypeDefinition() != typeof(MessageDispatcher<>))
                return null;

            return (T)GetMessageDispatcher(typeOfT.GetGenericArguments()[0]);
        }


        private bool GetMessageDispatcher(object message, out IMessageDispatcher dispatcher)
        {
            Type messageType = message.GetType();

        	_messageLock.AcquireReaderLock(Timeout.Infinite);
			try
			{
				return _messageDispatchers.TryGetValue(messageType, out dispatcher);
			}
			finally
			{
				_messageLock.ReleaseReaderLock();
			}
        }
    }
}