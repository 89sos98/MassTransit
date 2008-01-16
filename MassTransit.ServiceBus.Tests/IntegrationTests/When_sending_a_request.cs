using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace MassTransit.ServiceBus.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    public class When_sending_a_request
    {
        [Test]
        public void A_response_should_release_the_waiting_process()
        {
            using (QueueTestContext qtc = new QueueTestContext())
            {
                PingMessage ping = new PingMessage();

                qtc.ServiceBus.Subscribe<PingMessage>(
                    delegate(IMessageContext<PingMessage> context) { context.Reply(new PongMessage()); });

                IServiceBusAsyncResult asyncResult = qtc.ServiceBus.Request(qtc.ServiceBus.Endpoint, ping);

                Assert.That(asyncResult, Is.Not.Null);

                Assert.That(asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10), true), Is.True,
                            "Timeout Expired Waiting For Response");

                Assert.That(asyncResult.Messages, Is.Not.Null);

                Assert.That(asyncResult.Messages, Is.Not.Empty);

                
                PongMessage pong = asyncResult.Messages[0] as PongMessage;

                Assert.That(pong, Is.Not.Null);
            }
        }
    }
}