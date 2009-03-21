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
namespace MassTransit.Transports.Msmq.Tests
{
    using System;
    using System.Threading;
    using Magnum.DateTimeExtensions;
    using MassTransit.Tests;
    using MassTransit.Tests.Messages;
    using MassTransit.Tests.TestConsumers;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;
    using TestFixtures;

    [TestFixture]
    public class As_a_ServiceBus_working_with_an_endpoint :
        MsmqEndpointTestFixture

    {
        Uri inputAddress = new Uri("msmq://localhost/mt_client");
        Uri errorAddress = new Uri("msmq://localhost/mt_client_error");

        [Test]
        public void When_an_error_is_thrown_on_publish_the_bus_will_not_rollback_any_messages()
        {
            //publish ignores transactional semantics
        }

        [Test]
        public void When_an_error_is_encountered_while_publishing_only_the_errorendpoint_is_skipped()
        {
            
        }

        [Test]
        public void When_an_error_is_encountered_on_read_only_the_errorconsumers_get_skipped()
        {
            
        }

        [Test]
        public void When_a_consumer_errors_the_message_is_put_in_the_error_queue()
        {
            LocalBus.Subscribe(delegate(PingMessage msg)
                              {
                                  throw new NotSupportedException("POISON");
                              });
            new MsmqEndpoint(inputAddress).Send(new PingMessage());

            //new MsmqEndpoint(errorAddress).VerifyMessageInQueue<PingMessage>();
        }

        
    }
    
    [TestFixture]
    public class When_a_message_is_published_to_a_transactional_queue :
        MsmqTransactionalEndpointTestFixture
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        [Test]
        public void It_should_be_received_by_one_subscribed_consumer()
        {
            var consumer = new TestMessageConsumer<PingMessage>();
            RemoteBus.Subscribe(consumer);

            var message = new PingMessage();
            LocalBus.Publish(message);

            consumer.ShouldHaveReceivedMessage(message, _timeout);
        }

        [Test]
        public void It_should_leave_the_message_in_the_queue_if_an_exception_is_thrown()
        {
            RemoteBus.Subscribe<PingMessage>(m => { throw new ApplicationException("Boing!"); });

            var message = new PingMessage();
            LocalBus.Publish(message);
        }

        [Test]
        public void It_should_not_rollback_a_send_if_an_exception_is_thrown()
        {
            var consumer = new TestMessageConsumer<PongMessage>();
            LocalBus.Subscribe(consumer);

            var message = new PingMessage();
            var response = new PongMessage(message.CorrelationId);

            RemoteBus.Subscribe<PingMessage>(m =>
            {
                RemoteBus.Publish(response);
                throw new ApplicationException("Boing!");
            });

            LocalBus.Publish(message);

            consumer.ShouldHaveReceivedMessage(response, _timeout);
        }
    }

    [TestFixture]
    public class When_publishing_a_message :
        MsmqEndpointTestFixture

    {

        [Test]
        public void Multiple_Local_Services_Should_Be_Available()
        {
            ManualResetEvent _updateEvent = new ManualResetEvent(false);
            LocalBus.Subscribe<UpdateMessage>(msg => _updateEvent.Set());

            ManualResetEvent _deleteEvent = new ManualResetEvent(false);


            LocalBus.Subscribe<DeleteMessage>(
                delegate { _deleteEvent.Set(); });


            DeleteMessage dm = new DeleteMessage();

            LocalBus.Publish(dm);

            UpdateMessage um = new UpdateMessage();

            LocalBus.Publish(um);

            Assert.That(_deleteEvent.WaitOne(TimeSpan.FromSeconds(4), true), Is.True,
                        "Timeout expired waiting for message");

            Assert.That(_updateEvent.WaitOne(TimeSpan.FromSeconds(4), true), Is.True,
                        "Timeout expired waiting for message");

        }

        [Test]
        public void Multiple_messages_should_be_delivered_to_the_appropriate_remote_subscribers()
        {


            ManualResetEvent _updateEvent = new ManualResetEvent(false);

            RemoteBus.Subscribe<UpdateMessage>(
                delegate { _updateEvent.Set(); });

            ManualResetEvent _deleteEvent = new ManualResetEvent(false);

            RemoteBus.Subscribe<DeleteMessage>(
                delegate { _deleteEvent.Set(); });

            DeleteMessage dm = new DeleteMessage();

            LocalBus.Publish(dm);

            UpdateMessage um = new UpdateMessage();

            LocalBus.Publish(um);

            Assert.That(_deleteEvent.WaitOne(TimeSpan.FromSeconds(6), true), Is.True,
                        "Timeout expired waiting for message");

            Assert.That(_updateEvent.WaitOne(TimeSpan.FromSeconds(6), true), Is.True,
                        "Timeout expired waiting for message");

        }

        [Test]
        public void The_message_should_be_delivered_to_a_local_subscriber()
        {

            ManualResetEvent _updateEvent = new ManualResetEvent(false);

            LocalBus.Subscribe<UpdateMessage>(
                delegate { _updateEvent.Set(); });

            UpdateMessage um = new UpdateMessage();

            LocalBus.Publish(um);

            Assert.That(_updateEvent.WaitOne(TimeSpan.FromSeconds(3), true), Is.True,
                        "Timeout expired waiting for message");

        }

        [Test]
        public void The_message_should_be_delivered_to_a_remote_subscriber()
        {
           
                ManualResetEvent _updateEvent = new ManualResetEvent(false);

                RemoteBus.Subscribe<UpdateMessage>(
                    delegate { _updateEvent.Set(); });

                UpdateMessage um = new UpdateMessage();

                LocalBus.Publish(um);

                Assert.That(_updateEvent.WaitOne(TimeSpan.FromSeconds(3), true), Is.True,
                            "Timeout expired waiting for message");
            
        }

        [Test]
        public void The_message_should_be_delivered_to_a_remote_subscriber_with_a_reply()
        {
            ManualResetEvent _updateEvent = new ManualResetEvent(false);

            Action<UpdateMessage> handler =
                msg =>
                    {
                        _updateEvent.Set();

                        RemoteBus.Publish(new UpdateAcceptedMessage());
                    };

            ManualResetEvent _repliedEvent = new ManualResetEvent(false);

            RemoteBus.Subscribe(handler);

            LocalBus.Subscribe<UpdateAcceptedMessage>(
                delegate { _repliedEvent.Set(); });

            UpdateMessage um = new UpdateMessage();

            LocalBus.Publish(um);

            Assert.That(_updateEvent.WaitOne(TimeSpan.FromSeconds(3), true), Is.True,
                        "Timeout expired waiting for message");

            Assert.That(_repliedEvent.WaitOne(TimeSpan.FromSeconds(3), true), Is.True, "NO response message received");

        }
    }

    [TestFixture]
    public class When_an_accept_method_throws_an_exception :
        MsmqEndpointTestFixture
    {
        [Test]
        public void The_exception_should_not_disrupt_the_flow_of_messages()
        {
            MsmqEndpoint endpoint = new MsmqEndpoint("msmq://localhost/mt_client");

            CrashingService service = new CrashingService();

            LocalBus.Subscribe(service);

            endpoint.Send(new BogusMessage());

            CrashingService.Received.WaitOne(5.Seconds(), true).ShouldBeTrue("No message received");

            CrashingService.Received.Reset();

            endpoint.Send(new LegitMessage());

            CrashingService.LegitReceived.WaitOne(5.Seconds(), true).ShouldBeTrue("No message received");
        }

        internal class CrashingService :
            Consumes<BogusMessage>.All,
            Consumes<LegitMessage>.All
        {
            public static ManualResetEvent Received
            {
                get { return _received; }
            }

            private static readonly ManualResetEvent _received = new ManualResetEvent(false);

            public static ManualResetEvent LegitReceived
            {
                get { return _legitReceived; }
            }

            private static readonly ManualResetEvent _legitReceived = new ManualResetEvent(false);

            public void Consume(BogusMessage message)
            {
                _received.Set();

                throw new NotImplementedException();
            }

            public void Consume(LegitMessage message)
            {
                _legitReceived.Set();
            }
        }

        [Serializable]
        internal class BogusMessage
        {
        }

        [Serializable]
        internal class LegitMessage
        {
        }
    }

    [TestFixture]
    public class When_receiving_messages_slowly :
        MsmqEndpointTestFixture
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        [Test]
        public void It_should_be_received_by_one_subscribed_consumer()
        {
            var consumer = new TestMessageConsumer<PingMessage>();
            RemoteBus.Subscribe(consumer);

            Thread.Sleep(5.Seconds());

            var message = new PingMessage();
            LocalBus.Publish(message);

            consumer.ShouldHaveReceivedMessage(message, _timeout);
        }
    }
}