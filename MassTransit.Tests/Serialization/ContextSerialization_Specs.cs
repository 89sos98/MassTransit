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
namespace MassTransit.Tests.Serialization
{
	using Configuration;
	using Magnum.Common.DateTimeExtensions;
	using MassTransit.Serialization;
	using MassTransit.Transports;
	using Messages;
	using NUnit.Framework;
	using Rhino.Mocks;
	using TextFixtures;

	[TestFixture, Explicit]
	public class When_sending_a_message_using_the_specified_serializer<TSerializer> :
		EndpointTestFixture<LoopbackEndpoint>
		where TSerializer : IMessageSerializer
	{
		public IServiceBus LocalBus { get; private set; }
		public IServiceBus RemoteBus { get; private set; }

		protected override void EstablishContext()
		{
			base.EstablishContext();

			EndpointFactory = EndpointFactoryConfigurator.New(x =>
				{
					x.SetObjectBuilder(ObjectBuilder);
					x.RegisterTransport<LoopbackEndpoint>();
					x.SetDefaultSerializer<TSerializer>();
				});

			ObjectBuilder.Stub(x => x.GetInstance<IEndpointFactory>()).Return(EndpointFactory);

			LocalBus = ServiceBusConfigurator.New(x => { x.ReceiveFrom("loopback://localhost/mt_client"); });
			RemoteBus = ServiceBusConfigurator.New(x => { x.ReceiveFrom("loopback://localhost/mt_server"); });
		}

		protected override void TeardownContext()
		{
			LocalBus.Dispose();
			LocalBus = null;

			RemoteBus.Dispose();
			RemoteBus = null;

			base.TeardownContext();
		}

		[Test]
		public void The_destination_address_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			RemoteBus.Subscribe<PingMessage>(message =>
				{
					Assert.AreEqual(RemoteBus.Endpoint.Uri, CurrentMessage.DestinationAddress);

					received.Set(message);
				});

			LocalBus.Publish(ping);

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

		[Test]
		public void The_fault_address_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			RemoteBus.Subscribe<PingMessage>(message =>
				{
					Assert.AreEqual(LocalBus.Endpoint.Uri, CurrentMessage.FaultAddress);

					received.Set(message);
				});

			LocalBus.Publish(ping, context => context.SendFaultTo(LocalBus.Endpoint.Uri));

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

		[Test]
		public void The_response_address_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			RemoteBus.Subscribe<PingMessage>(message =>
				{
					Assert.AreEqual(LocalBus.Endpoint.Uri, CurrentMessage.ResponseAddress);

					received.Set(message);
				});

			LocalBus.Publish(ping, context => context.SendResponseTo(LocalBus.Endpoint.Uri));

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

		[Test]
		public void The_retry_count_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			var retryCount = 69;
			RemoteBus.Subscribe<PingMessage>(message =>
				{
					Assert.AreEqual(retryCount, CurrentMessage.RetryCount);

					received.Set(message);
				});

			LocalBus.Publish(ping, context => context.SetRetryCount(retryCount));

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

		[Test]
		public void The_source_address_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			RemoteBus.Subscribe<PingMessage>(message =>
				{
					Assert.AreEqual(LocalBus.Endpoint.Uri, CurrentMessage.SourceAddress);

					received.Set(message);
				});

			LocalBus.Publish(ping);

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

		[Test]
		public void The_message_type_should_be_properly_set_on_the_message_envelope()
		{
			PingMessage ping = new PingMessage();

			FutureMessage<PingMessage> received = new FutureMessage<PingMessage>();

			RemoteBus.Subscribe<PingMessage>(message =>
			{
				Assert.AreEqual(MessageEnvelopeBase.FormatMessageType(typeof(PingMessage)), CurrentMessage.MessageType);

				received.Set(message);
			});

			LocalBus.Publish(ping);

			Assert.IsTrue(received.IsAvailable(3.Seconds()), "Timeout waiting for message");
		}

	}

	[TestFixture]
	public class For_the_binary_message_serializer :
		When_sending_a_message_using_the_specified_serializer<BinaryMessageSerializer>
	{
	}
}