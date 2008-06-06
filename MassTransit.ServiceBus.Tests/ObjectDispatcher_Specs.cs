namespace MassTransit.ServiceBus.Tests
{
	using Internal;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;

	[TestFixture]
	public class When_a_type_is_registered_with_the_dispatcher
	{
		[Test]
		public void A_new_object_should_be_created_to_handle_each_message()
		{
			IObjectBuilder builder = new ActivatorObjectBuilder();

			MessageDispatcher dispatcher = new MessageDispatcher(null, null, builder);

			dispatcher.AddComponent<RequestHandler>();
			dispatcher.AddComponent<SelectiveHandler>();

			TestMessage message = new TestMessage(27);

			dispatcher.Consume(message);

			Assert.That(RequestHandler.Value, Is.EqualTo(27));
			Assert.That(SelectiveHandler.Value, Is.EqualTo(default(int)));
		}

		[Test]
		public void A_new_object_should_be_created_to_handle_each_message_including_selective_ones()
		{
			IObjectBuilder builder = new ActivatorObjectBuilder();

			MessageDispatcher dispatcher = new MessageDispatcher(null, null, builder);

			dispatcher.AddComponent<RequestHandler>();
			dispatcher.AddComponent<SelectiveHandler>();

			TestMessage message = new TestMessage(42);

			dispatcher.Consume(message);

			Assert.That(RequestHandler.Value, Is.EqualTo(42));
			Assert.That(SelectiveHandler.Value, Is.EqualTo(42));
		}

		internal class RequestHandler : Consumes<TestMessage>.All
		{
			private static int _value;

			public static int Value
			{
				get { return _value; }
			}

			public void Consume(TestMessage message)
			{
				_value = message.Value;
			}
		}

		internal class SelectiveHandler : Consumes<TestMessage>.Selected
		{
			private static int _value;

			public static int Value
			{
				get { return _value; }
			}

			public bool Accept(TestMessage message)
			{
				return message.Value > 27;
			}

			public void Consume(TestMessage message)
			{
				_value = message.Value;
			}
		}

		internal class TestMessage
		{
			private readonly int _value;

			public TestMessage(int value)
			{
				_value = value;
			}

			public int Value
			{
				get { return _value; }
			}
		}
	}
}