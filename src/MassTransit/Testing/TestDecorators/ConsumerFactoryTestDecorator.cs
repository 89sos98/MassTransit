// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Testing.TestDecorators
{
	using System;
	using System.Collections.Generic;
	using Subjects;

	public class ConsumerFactoryTestDecorator<TConsumer> :
		IConsumerFactory<TConsumer>
		where TConsumer : class
	{
		readonly IServiceBus _bus;
		readonly IConsumerFactory<TConsumer> _consumerFactory;
		readonly ReceivedMessageList _received;

		public ConsumerFactoryTestDecorator(IConsumerFactory<TConsumer> consumerFactory, IServiceBus bus,
		                                    ReceivedMessageList received)
		{
			_consumerFactory = consumerFactory;
			_bus = bus;
			_received = received;
		}

		public IEnumerable<Action<TMessage>> GetConsumer<TMessage>(Func<TConsumer, Action<TMessage>> callback)
			where TMessage : class
		{
			IEnumerable<Action<TMessage>> consumers = _consumerFactory.GetConsumer(callback);

			foreach (var action in consumers)
			{
				Action<TMessage> consumer = action;

				yield return message =>
					{
						var received = new ReceivedMessage<TMessage>(_bus.MessageContext<TMessage>(), message);

						try
						{
							consumer(message);
						}
						catch (Exception ex)
						{
							received.SetException(ex);
						}
						finally
						{
							_received.Add(received);
						}
					};
			}
		}
	}
}