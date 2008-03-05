namespace MassTransit.Patterns.Tests
{
	using System;
	using System.Threading;
	using Batching;
	using log4net;
	using MassTransit.ServiceBus.Internal;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;
	using Rhino.Mocks;
	using ServiceBus;

	[TestFixture]
	public class As_a_BatchController2
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_log.Info("Here we go!");

			_mocks = new MockRepository();

			_endpoint = _mocks.DynamicMock<IEndpoint>();
			_receiver = _mocks.DynamicMock<IMessageReceiver>();

			SetupResult.For(_endpoint.Uri).Return(new Uri("msmq://localhost/test_queue"));
			SetupResult.For(_endpoint.Receiver).Return(_receiver);

			_mocks.ReplayAll();
			_bus = new ServiceBus(_endpoint);
		}

		#endregion

		private static readonly ILog _log = LogManager.GetLogger(typeof (As_a_BatchController2));
		private MockRepository _mocks;
		private IEndpoint _endpoint;
		private IMessageReceiver _receiver;
		private ServiceBus _bus;

		[Test]
		public void A_missing_message_should_leave_the_batch_incomplete()
		{
			bool wasCalled = false;
			bool isComplete = false;

			BatchController2<StringBatchMessage, Guid> c = new BatchController2<StringBatchMessage, Guid>(
				delegate(IBatchContext<StringBatchMessage, Guid> cxt)
					{
						foreach (StringBatchMessage msg in cxt)
						{
							//this shouldn't be called because we don't do anything until we get them all
							wasCalled = true;
							isComplete = cxt.IsComplete;
						}
					}, TimeSpan.FromSeconds(3));

			Guid batchId = Guid.NewGuid();
			int batchLength = 2;

			StringBatchMessage msg1 = new StringBatchMessage(batchId, batchLength, "hello");

			IEnvelope env1 = new Envelope(msg1);

			_bus.Subscribe<StringBatchMessage>(c.HandleMessage);

			_bus.Deliver(env1);

			Assert.That(wasCalled, Is.False, "Was Called");
			Assert.That(isComplete, Is.False, "Not Complete");
		}

		[Test, Ignore("Time out not implemented")]
		public void A_timeout_should_leave_the_batch_incomplete() //should it do more?
		{
			bool wasCalled = false;
			bool isComplete = false;

			BatchController2<StringBatchMessage, Guid> c = new BatchController2<StringBatchMessage, Guid>(
				delegate(IBatchContext<StringBatchMessage, Guid> cxt)
					{
						foreach (StringBatchMessage msg in cxt)
						{
							//this shouldn't be called because we don't do anything until we get them all
							wasCalled = true;
							isComplete = cxt.IsComplete;
						}
					}, TimeSpan.FromSeconds(3));

			Guid batchId = Guid.NewGuid();
			int batchLength = 2;

			StringBatchMessage msg1 = new StringBatchMessage(batchId, batchLength, "hello");

			IEnvelope env1 = new Envelope(msg1);

			_bus.Subscribe<StringBatchMessage>(c.HandleMessage);

			_bus.Deliver(env1);
			Thread.Sleep(3005);
			_bus.Deliver(env1);

			Assert.That(wasCalled, Is.False, "Was Called");
			Assert.That(isComplete, Is.False, "Not Complete");
		}

		[Test]
		public void Multiple_messages_should_be_complete_when_the_last_message_is_received()
		{
			bool wasCalled = false;
			bool isComplete = false;
			int numberCalled = 0;

			BatchController2<StringBatchMessage, Guid> c = new BatchController2<StringBatchMessage, Guid>(
				delegate(IBatchContext<StringBatchMessage, Guid> cxt)
					{
						numberCalled = 0;

						foreach (StringBatchMessage msg in cxt)
						{
							wasCalled = true;
							isComplete = cxt.IsComplete;
							numberCalled++;
						}
					}, TimeSpan.FromSeconds(3));

			Guid batchId = Guid.NewGuid();
			int batchLength = 4;

			StringBatchMessage msg1 = new StringBatchMessage(batchId, batchLength, "hello");
			StringBatchMessage msg2 = new StringBatchMessage(batchId, batchLength, "hello");
			StringBatchMessage msg3 = new StringBatchMessage(batchId, batchLength, "hello");
			StringBatchMessage msg4 = new StringBatchMessage(batchId, batchLength, "hello");

			IEnvelope env1 = new Envelope(msg1);
			IEnvelope env2 = new Envelope(msg2);
			IEnvelope env3 = new Envelope(msg3);
			IEnvelope env4 = new Envelope(msg4);

			_bus.Subscribe<StringBatchMessage>(c.HandleMessage);

			ManualResetEvent started = new ManualResetEvent(false);
			ManualResetEvent done = new ManualResetEvent(false);

			ThreadPool.QueueUserWorkItem(delegate
			                             	{
			                             		started.Set();
			                             		_bus.Deliver(env1);
			                             		done.Set();
			                             	});

			started.WaitOne(TimeSpan.FromSeconds(3), true);

			_bus.Deliver(env2);
			_bus.Deliver(env3);
			_bus.Deliver(env4);

			done.WaitOne(TimeSpan.FromSeconds(10), true);

			Assert.That(wasCalled, Is.True, "Not Called");
			Assert.That(numberCalled, Is.EqualTo(4));
			Assert.That(isComplete, Is.True, "Not Complete");
		}

		[Test]
		public void The_batch_should_be_complete_when_the_last_message_is_received()
		{
			bool wasCalled = false;
			bool isComplete = false;

			BatchController2<StringBatchMessage, Guid> c = new BatchController2<StringBatchMessage, Guid>(
				delegate(IBatchContext<StringBatchMessage, Guid> cxt)
					{
						foreach (StringBatchMessage msg in cxt)
						{
							wasCalled = true;
							isComplete = cxt.IsComplete;
						}
					}, TimeSpan.FromSeconds(3));


			Guid batchId = Guid.NewGuid();
			int batchLength = 1;

			StringBatchMessage msg1 = new StringBatchMessage(batchId, batchLength, "hello");

			IEnvelope env1 = new Envelope(msg1);

			_bus.Subscribe<StringBatchMessage>(c.HandleMessage);

			_bus.Deliver(env1);

			Assert.That(wasCalled, Is.True, "Not Called");
			Assert.That(isComplete, Is.True, "Not Complete");
		}
	}
}