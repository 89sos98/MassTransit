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
namespace MassTransit.Tests
{
	using System;
	using Messages;
	using NUnit.Framework;
	using TextFixtures;

	[TestFixture]
	public class MessageContext_Specs :
		LoopbackTestFixture
	{
		[Test]
		public void A_consumer_should_be_able_to_get_at_the_headers()
		{
			LocalBus.Subscribe<PingMessage>(message => { CurrentMessage.Reply(new PongMessage(message.CorrelationId)); });
		}

		[Test]
		public void First_test()
		{
			var correlationId = Guid.NewGuid();

			var pong = new PongMessage(correlationId);

//			LocalBus.Publish(pong, x =>
//				{
//					x.InResponseTo(correlationId.ToString());
//				});
//
//
//			var command = new UpdateMessage();
//
//			Uri _someUri= new Uri("loopback://localhost/mt_client");
//			Uri _someOtherUri = new Uri("loopback://localhost/mt_client");
//
//			LocalBus.Publish(command, x =>
//				{
//					x.SendFaultsTo(_someUri);
//					x.SendResponsesTo(_someOtherUri);
//				});
//
//			LocalBus.Publish(command);
		}
	}


//
//	public static class MoreExt
//	{
//		public static void Publish<T>(this IServiceBus bus, T message, Action<IMessageContext> action)
//		{
//			
//		}
//	}
}