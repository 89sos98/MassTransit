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
namespace MassTransit.Distributor
{
	using System;
	using System.Threading;
	using Internal;
	using Magnum.Actors;
	using Magnum.Actors.CommandQueues;
	using Messages;

	public class Worker<T> :
		IWorker<T>,
		Consumes<WakeUpWorker>.All,
		Consumes<Distributed<T>>.Selected
		where T : class
	{
		private int _pending;
		private IServiceBus _bus;
		private IServiceBus _controlBus;
		private Func<T, Action<T>> _getConsumer;
		private int _inProgress;
		private int _inProgressLimit = 4;
		private int _pendingLimit = 16;
		private UnsubscribeAction _unsubscribeAction = () => false;
		private Uri _dataUri;
		private Uri _controlUri;
		private CommandQueue _queue = new ThreadPoolCommandQueue();
        private WorkerPendingMessageTracker<Guid> _pendingMessages = new WorkerPendingMessageTracker<Guid>();

		public Worker(Func<T, Action<T>> getConsumer)
			: this(getConsumer, new WorkerSettings())
		{
		}

		public Worker(Func<T, Action<T>> getConsumer, WorkerSettings settings)
		{
			_getConsumer = getConsumer;

			_inProgress = 0;
			_inProgressLimit = settings.InProgressLimit;
			_pending = 0;
			_pendingLimit = settings.PendingLimit;
		}

		public void Consume(Distributed<T> message)
		{
            _pendingMessages.Consumed(message.CorrelationId);

			Action<T> consumer = _getConsumer(message.Payload);

			Interlocked.Increment(ref _inProgress);
			try
			{
				RewriteResponseAddress(message.ResponseAddress);

				consumer(message.Payload);
			}
			finally
			{
				Interlocked.Decrement(ref _inProgress);

				if (_inProgress == 0)
				{
					_queue.Enqueue(PublishWorkerAvailability);
					_bus.Endpoint.Send(new WakeUpWorker());
				}

				var disposal = consumer as IDisposable;
				if (disposal != null)
				{
					disposal.Dispose();
				}
			}
		}

		public bool Accept(Distributed<T> message)
		{
            _pendingMessages.Viewed(message.CorrelationId);

			if (_inProgress >= _inProgressLimit)
				return false;

			return true;
		}

		public void Dispose()
		{
			_controlBus = null;
			_getConsumer = null;
		}

		public void Start(IServiceBus bus)
		{
			_bus = bus;
			_controlBus = bus.ControlBus;

			_dataUri = _bus.Endpoint.Uri;
			_controlUri = _controlBus.Endpoint.Uri;

			_unsubscribeAction = bus.ControlBus.Subscribe<ConfigureWorker<T>>(Consume);
			_unsubscribeAction += bus.Subscribe(this);

			PublishWorkerAvailability();
		}

		public void Stop()
		{
			_unsubscribeAction();
		}

		private void Consume(ConfigureWorker<T> message)
		{
			if (message.InProgressLimit >= 0)
				_inProgressLimit = message.InProgressLimit;

			if (message.PendingLimit >= 0)
				_pendingLimit = message.PendingLimit;

			PublishWorkerAvailability();
		}

		private void PublishWorkerAvailability()
		{
		    _pending = _pendingMessages.PendingMessagesCount();
			_bus.Publish(new WorkerAvailable<T>(_controlUri, _dataUri, _inProgress, _inProgressLimit, _pending, _pendingLimit));
		}

		private static void RewriteResponseAddress(Uri responseAddress)
		{
			InboundMessageHeaders.SetCurrent(x => x.SetResponseAddress(responseAddress));
		}

		public void Consume(WakeUpWorker message)
		{
		}
	}
}