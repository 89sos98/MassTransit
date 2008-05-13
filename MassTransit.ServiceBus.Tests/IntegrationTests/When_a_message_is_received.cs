namespace MassTransit.ServiceBus.Tests.IntegrationTests
{
    using System;
    using System.Threading;
    using Internal;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    [Explicit]
    [TestFixture]
    public class When_a_message_is_received :
        ServiceBusSetupFixture
    {
        [Test]
        public void An_Event_Handler_Should_Be_Called()
        {
            bool _received = false;
            ManualResetEvent _receivedEvent = new ManualResetEvent(false);

            Action<IMessageContext<PingMessage>> handler = delegate
                                                               {
                                                                   _received = true;
                                                                   _receivedEvent.Set();
                                                               };

            _serviceBus.Subscribe(handler);

            PingMessage pm = new PingMessage();
            _serviceBus.Publish(pm);

            Assert.That(_receivedEvent.WaitOne(TimeSpan.FromSeconds(5), true), Is.True);

            Assert.That(_received, Is.True);
        }

        [Test]
        public void What_Happens_If_No_Subscriptions()
        {
            bool _received = false;
            ManualResetEvent _receivedEvent = new ManualResetEvent(false);

            PingMessage pm = new PingMessage();
            _serviceBus.Publish(pm);

            Assert.That(_receivedEvent.WaitOne(TimeSpan.FromSeconds(5), true), Is.False);

            Assert.That(_received, Is.False);
        }
    }
}