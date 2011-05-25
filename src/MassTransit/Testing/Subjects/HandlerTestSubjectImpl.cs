﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Testing.Subjects
{
	using System;

	public class HandlerTestSubjectImpl<T> :
		HandlerTestSubject<T>
		where T : class
	{
		readonly ReceivedMessageList<T> _received;
		IServiceBus _bus;
		bool _disposed;
		UnsubscribeAction _unsubscribe;

		public HandlerTestSubjectImpl()
		{
			_received = new ReceivedMessageList<T>();
		}

		public IReceivedMessageList<T> Received
		{
			get { return _received; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Prepare(IServiceBus bus)
		{
			_bus = bus;

			_unsubscribe = bus.SubscribeHandler<T>(HandleMessage);
		}

		void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				if (_unsubscribe != null)
				{
					_unsubscribe();
					_unsubscribe = null;
				}

				_received.Dispose();
			}

			_disposed = true;
		}

		void HandleMessage(T message)
		{
			var received = new ReceivedMessage<T>(_bus.MessageContext<T>(), message);

			_received.Add(received);
		}

		~HandlerTestSubjectImpl()
		{
			Dispose(false);
		}
	}
}