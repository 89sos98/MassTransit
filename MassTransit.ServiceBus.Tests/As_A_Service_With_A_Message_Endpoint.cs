using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using MassTransit.ServiceBus.Tests.Messages;

namespace MassTransit.ServiceBus.Tests
{
    using Rhino.Mocks;

    [TestFixture]
	public class As_A_Service_With_A_Message_Endpoint
	{
        private ServiceBus _serviceBus;
        private MockRepository mocks;
        private IEndpoint mockEndpoint;
        private ISubscriptionStorage mockSubscriptionStorage;


        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            mockEndpoint = mocks.CreateMock<IEndpoint>();
            mockSubscriptionStorage = mocks.CreateMock<ISubscriptionStorage>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks = null;
            _serviceBus = null;
        }

		[Test]
		public void I_Want_To_Be_Able_To_Register_An_Event_Handler_For_Messages()
		{
            using(mocks.Record())
            {
                Expect.Call(mockEndpoint.Address).Return(@".\private$\endpointa");
                mockEndpoint.EnvelopeReceived += delegate { };
                LastCall.IgnoreArguments();
                mockSubscriptionStorage.Add(typeof(PingMessage), mockEndpoint);
            }

            using(mocks.Playback())
            {
                _serviceBus = new ServiceBus(mockEndpoint);
                _serviceBus.SubscriptionStorage = mockSubscriptionStorage;

                _serviceBus.MessageEndpoint<PingMessage>().Subscribe(MyUpdateMessage_Received);
            }

            Assert.That(_serviceBus.MessageEndpoint<PingMessage>(), Is.Not.Null);
		}

        private static void MyUpdateMessage_Received(MessageContext<PingMessage> ctx)
		{
		}
	}
}