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
	using Magnum.Common.DateTimeExtensions;
	using MassTransit.Tests;
	using MassTransit.Tests.Messages;
	using NUnit.Framework;
	using TestFixtures;

	[TestFixture]
	public class Given_a_message_is_received_from_a_nontransactional_queue :
		MsmqEndpointTestFixture
	{
		protected override void EstablishContext()
		{
			using (var endpoint = new MsmqEndpoint("msmq://localhost/mt_client"))
				endpoint.Purge();
			using (var endpoint = new MsmqEndpoint("msmq://localhost/mt_client_error"))
				endpoint.Purge();
			using (var endpoint = new MsmqEndpoint("msmq://localhost/mt_server"))
				endpoint.Purge();

			base.EstablishContext();
		}
	}

	[TestFixture]
	public class When_a_consumer_throws_an_exception :
		Given_a_message_is_received_from_a_nontransactional_queue
	{
		private PingMessage _ping;

		protected override void EstablishContext()
		{
			base.EstablishContext();

			LocalBus.Subscribe<PingMessage>(message => { throw new NotSupportedException("I am a naughty consumer! I go boom!"); });

			_ping = new PingMessage();

			LocalBus.Publish(_ping);
		}

		[Test]
		public void The_message_should_exist_in_the_error_queue()
		{
			LocalErrorEndpoint.ShouldContain(_ping);
		}

		[Test]
		public void The_message_should_not_exist_in_the_input_queue()
		{
			LocalErrorEndpoint.ShouldContain(_ping);

			LocalEndpoint.ShouldNotContain(_ping);
		}
	}
	
	[TestFixture]
	public class When_a_consumer_receives_the_message :
		Given_a_message_is_received_from_a_nontransactional_queue
	{
		private PingMessage _ping;
		private FutureMessage<PingMessage> _future;

		protected override void EstablishContext()
		{
			base.EstablishContext();

			_future = new FutureMessage<PingMessage>();

			LocalBus.Subscribe<PingMessage>(message => _future.Set(message));

			_ping = new PingMessage();

			LocalBus.Publish(_ping);
		}

		[Test]
		public void The_message_should_not_exist_in_the_error_queue()
		{
			Assert.IsTrue(_future.IsAvailable(3.Seconds()));

			LocalErrorEndpoint.ShouldNotContain(_ping);
		}

		[Test]
		public void The_message_should_not_exist_in_the_input_queue()
		{
			Assert.IsTrue(_future.IsAvailable(3.Seconds()));

			LocalEndpoint.ShouldNotContain(_ping);
		}
	}
}