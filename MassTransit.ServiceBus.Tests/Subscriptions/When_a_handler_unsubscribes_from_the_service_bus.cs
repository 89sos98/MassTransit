namespace MassTransit.ServiceBus.Tests.Subscriptions
{
	using System;
	using Internal;
	using MassTransit.ServiceBus.Subscriptions;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;
	using Rhino.Mocks;

	[TestFixture]
	public class When_a_handler_unsubscribes_from_the_service_bus
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_endpoint = _mocks.CreateMock<IEndpoint>();
			_cache = _mocks.CreateMock<ISubscriptionCache>();


			_bus = new ServiceBus(_endpoint, _cache);
			_receiver = _mocks.CreateMock<IMessageReceiver>();
		}

		[TearDown]
		public void Teardown()
		{
		}

		#endregion

		private MockRepository _mocks = new MockRepository();
		private IEndpoint _endpoint;
		private ISubscriptionCache _cache;
		private ServiceBus _bus;
		private IEnvelopeConsumer _consumer;
		private IEnvelope _envelope = new Envelope(new PingMessage());
		private Uri _endpointUri = new Uri("msmq://localhost/test");
		private IMessageReceiver _receiver;


		private static void HandleAllMessages(IMessageContext<PingMessage> ctx)
		{
		}

		private static bool HandleSomeMessagesPredicate(PingMessage message)
		{
			return true;
		}

		[Test]
		public void The_service_bus_should_continue_to_handle_messages_if_at_least_one_handler_is_available()
		{
			using (_mocks.Record())
			{
                Expect.Call(_endpoint.Uri).Return(_endpointUri).Repeat.Any();
                Expect.Call(delegate { _cache.Add(null); }).IgnoreArguments();
                Expect.Call(_endpoint.Receiver).Return(_receiver);
                Expect.Call(delegate { _receiver.Subscribe(_consumer); }).IgnoreArguments();

                Expect.Call(delegate { _cache.Add(null); }).IgnoreArguments();
                Expect.Call(_endpoint.Receiver).Return(_receiver);
                Expect.Call(delegate { _receiver.Subscribe(_consumer); }).IgnoreArguments();

                Expect.Call(delegate { _cache.Remove(null); }).IgnoreArguments();
                Expect.Call(delegate { _cache.Remove(null); }).IgnoreArguments();
			}

			using (_mocks.Playback())
			{
				_consumer = _bus as IEnvelopeConsumer;
				_bus.Subscribe<PingMessage>(HandleAllMessages);
				Assert.That(_consumer.IsInterested(_envelope), Is.True);

				_bus.Subscribe<PingMessage>(HandleAllMessages, HandleSomeMessagesPredicate);
				Assert.That(_consumer.IsInterested(_envelope), Is.True);

				_bus.Unsubscribe<PingMessage>(HandleAllMessages);
				Assert.That(_consumer.IsInterested(_envelope), Is.True);

				_bus.Unsubscribe<PingMessage>(HandleAllMessages, HandleSomeMessagesPredicate);
				Assert.That(_consumer.IsInterested(_envelope), Is.False);
			}
		}

		[Test]
		public void The_service_bus_should_no_longer_show_the_message_type_as_handled()
		{
			using (_mocks.Record())
			{
				Expect.Call(_endpoint.Uri).Return(_endpointUri).Repeat.Any();
				Expect.Call(_endpoint.Receiver).Return(_receiver);
				Expect.Call(delegate { _receiver.Subscribe(_consumer); }).IgnoreArguments();
				Expect.Call(delegate { _cache.Add(null); }).IgnoreArguments();

				Expect.Call(delegate { _cache.Remove(null); }).IgnoreArguments();
			}

			using (_mocks.Playback())
			{
				_consumer = _bus as IEnvelopeConsumer;

				_bus.Subscribe<PingMessage>(HandleAllMessages);
				Assert.That(_consumer.IsInterested(_envelope), Is.True);

				_bus.Unsubscribe<PingMessage>(HandleAllMessages);
				Assert.That(_consumer.IsInterested(_envelope), Is.False);
			}
		}
	}
}