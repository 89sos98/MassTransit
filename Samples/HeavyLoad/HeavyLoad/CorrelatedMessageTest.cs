namespace HeavyLoad
{
	using System;
	using System.Threading;
	using MassTransit.ServiceBus;
	using MassTransit.ServiceBus.MSMQ;

	public class CorrelatedMessageTest : IDisposable
	{
		private readonly ManualResetEvent _finishedEvent = new ManualResetEvent(false);
		private readonly string _queueUri = "msmq://localhost/test_servicebus";
		private int _attempts = 100;
		private IServiceBus _bus;
		private MsmqEndpoint _localEndpoint;
		private int _successes;
		private int _timeouts;

		public CorrelatedMessageTest()
		{
			_localEndpoint = new MsmqEndpoint(_queueUri);

			MsmqHelper.ValidateAndPurgeQueue(_localEndpoint.QueuePath);

			_bus = new ServiceBus(_localEndpoint);
		}

		public void Dispose()
		{
			if (_bus != null)
			{
				_bus.Dispose();
				_bus = null;
			}

			if (_localEndpoint != null)
			{
				_localEndpoint.Dispose();
				_localEndpoint = null;
			}
		}

		public void Run(StopWatch stopWatch)
		{
			stopWatch.Start();

			ThreadPool.SetMaxThreads(256, 1000);

			SimpleRequestService service = new SimpleRequestService(_bus);

			_bus.Subscribe(service);

			Thread.Sleep(1000);

			for (int index = 0; index < 10; index++)
			{
				ThreadPool.QueueUserWorkItem(delegate
				                             	{
				                             		CheckPoint point = stopWatch.Mark("Thread " + index);

													for (int indexer = 0; indexer < _attempts / 10; indexer++)
													{
														CorrelatedController controller = new CorrelatedController(_bus);

														controller.OnSuccess += controller_OnSuccess;
														controller.OnTimeout += controller_OnTimeout;
														controller.SimulateRequestResponse();

														controller.OnSuccess -= controller_OnSuccess;
														controller.OnTimeout -= controller_OnTimeout;
													}

				                             		point.Complete(_attempts/10);
				                             	});
			}

			_finishedEvent.WaitOne(TimeSpan.FromSeconds(60), true);

			_bus.Unsubscribe(service);

			Console.WriteLine("Attempts: {0}, Succeeded: {1}, Timeouts: {2}", _attempts, _successes, _timeouts);

			stopWatch.Stop();
		}

		private void controller_OnTimeout(CorrelatedController obj)
		{
			_timeouts++;

			if (_timeouts + _successes == _attempts)
				_finishedEvent.Set();
		}

		private void controller_OnSuccess(CorrelatedController obj)
		{
			_successes++;

			if (_timeouts + _successes == _attempts)
				_finishedEvent.Set();
		}
	}

	internal class SimpleRequestService : Consumes<SimpleRequestMessage>.Any
	{
		private readonly IServiceBus _bus;

		public SimpleRequestService(IServiceBus bus)
		{
			_bus = bus;
		}

		public void Consume(SimpleRequestMessage message)
		{
			_bus.Publish(new SimpleResponseMessage(message.CorrelationId));
		}
	}

	internal class CorrelatedController : Consumes<SimpleResponseMessage>.For<Guid>
	{
		private readonly IServiceBus _bus;
		private readonly Guid _id;
		private readonly ManualResetEvent _responseEvent = new ManualResetEvent(false);


		public CorrelatedController(IServiceBus bus)
		{
			_bus = bus;

			_id = Guid.NewGuid();
		}

		public void Consume(SimpleResponseMessage message)
		{
			_responseEvent.Set();
		}

		public Guid CorrelationId
		{
			get { return _id; }
		}

		public event Action<CorrelatedController> OnSuccess = delegate { };
		public event Action<CorrelatedController> OnTimeout = delegate { };

		public void SimulateRequestResponse()
		{
			_bus.Subscribe(this);

			_bus.Publish(new SimpleRequestMessage(_id));

			bool success = _responseEvent.WaitOne(TimeSpan.FromSeconds(60), true);
			if (success)
				OnSuccess(this);
			else
				OnTimeout(this);

			_bus.Unsubscribe(this);
		}
	}

	[Serializable]
	internal class SimpleRequestMessage : CorrelatedBy<Guid>, IMessage
	{
		private readonly Guid _id;

		public SimpleRequestMessage(Guid id)
		{
			_id = id;
		}

		public Guid CorrelationId
		{
			get { return _id; }
		}
	}

	[Serializable]
	internal class SimpleResponseMessage : CorrelatedBy<Guid>, IMessage
	{
		private readonly Guid _id;

		public SimpleResponseMessage(Guid id)
		{
			_id = id;
		}

		public Guid CorrelationId
		{
			get { return _id; }
		}
	}
}