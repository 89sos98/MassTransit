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
namespace MassTransit.Internal.RequestResponse
{
	using System;

	public class ConditionalResponseAction<T> :
		IResponseAction,
		Consumes<T>.Selected
		where T : class
	{
		private readonly Func<T, bool> _accept;
		private readonly Action<T> _responseAction;
		private readonly RequestResponseScope _scope;

		public ConditionalResponseAction(RequestResponseScope scope, Func<T, bool> accept, Action<T> action)
		{
			_scope = scope;
			_accept = accept;
			_responseAction = action;
		}

		public void Consume(T message)
		{
			_responseAction(message);

			_scope.SetResponseReceived(message);
		}

		public bool Accept(T message)
		{
			return _accept(message);
		}

		public UnsubscribeAction SubscribeTo(IServiceBus bus)
		{
			return bus.Subscribe(this);
		}
	}
}