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
	using System.Collections.Generic;
	using System.Threading;

	public class RequestResponseScope :
		IAsyncResult
	{
		private readonly IServiceBus _bus;
		private readonly Action<IServiceBus> _requestAction;
		private readonly List<IResponseAction> _responseActions = new List<IResponseAction>();
		private readonly ManualResetEvent _responseReceived = new ManualResetEvent(false);
		private TimeSpan _responseTimeout = TimeSpan.MaxValue;
		private object _state;
		private Action _timeoutAction;
		private RegisteredWaitHandle _waitHandle;

		public RequestResponseScope(IServiceBus bus, Action<IServiceBus> requestAction)
		{
			_bus = bus;
			_requestAction = requestAction;
		}

		public bool IsCompleted
		{
			get { return _responseReceived.WaitOne(0, false); }
		}

		public WaitHandle AsyncWaitHandle
		{
			get { return _responseReceived; }
		}

		public object AsyncState
		{
			get { return _state; }
		}

		public bool CompletedSynchronously
		{
			get { return false; }
		}

		public void AddResponseAction(IResponseAction responseAction)
		{
			_responseActions.Add(responseAction);
		}

		public void Send()
		{
			UnsubscribeAction unsubscribeToken = () => true;
			try
			{
				unsubscribeToken = SubscribeToResponseMessages(unsubscribeToken);

				InvokeRequestAction();

				if (WaitForResponseAction() == false)
				{
					if (_timeoutAction != null)
						_timeoutAction();
				}
			}
			finally
			{
				unsubscribeToken();
			}
		}

		public RequestResponseScope TimeoutAfter(TimeSpan span)
		{
			_responseTimeout = span;

			return this;
		}

		public void SetResponseReceived<TMessage>(TMessage message)
			where TMessage : class
		{
			_responseReceived.Set();
		}

		public RequestResponseScope OnTimeout(Action action)
		{
			_timeoutAction = action;

			return this;
		}

		public IAsyncResult BeginSend(AsyncCallback callback, object state)
		{
			_state = state;

			UnsubscribeAction unsubscribeToken = () => true;
			unsubscribeToken = SubscribeToResponseMessages(unsubscribeToken);

			WaitOrTimerCallback timerCallback = (s, timedOut) =>
				{
					unsubscribeToken();

					if (timedOut)
					{
						if (_timeoutAction != null)
							_timeoutAction();
					}

					callback(this);
				};

			_waitHandle = ThreadPool.RegisterWaitForSingleObject(_responseReceived, timerCallback, state, _responseTimeout, true);

			InvokeRequestAction();

			return this;
		}

		private bool WaitForResponseAction()
		{
			return _responseReceived.WaitOne(_responseTimeout, true);
		}

		private void InvokeRequestAction()
		{
			_requestAction(_bus);
		}

		private UnsubscribeAction SubscribeToResponseMessages(UnsubscribeAction unsubscribeToken)
		{
			for (int i = 0; i < _responseActions.Count; i++)
			{
				unsubscribeToken += _responseActions[i].SubscribeTo(_bus);
			}
			return unsubscribeToken;
		}

		~RequestResponseScope()
		{
			if (_waitHandle != null)
			{
				_waitHandle.Unregister(_responseReceived);
				_waitHandle = null;
			}

			_responseReceived.Close();
		}
	}
}