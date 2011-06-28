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
namespace MassTransit.Diagnostics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Context;
	using Events;
	using log4net;
	using Stact;

	public class MessageTraceBusService :
		IBusService,
		Consumes<GetMessageTraceList>.All
	{
		static readonly ILog _log = LogManager.GetLogger(typeof (MessageTraceBusService));
		readonly ChannelConnection _connection;
		readonly UntypedChannel _eventChannel;
		readonly LinkedList<MessageTraceDetail> _messageList;
		IServiceBus _controlBus;
		int _detailLimit;
		Fiber _fiber;
		UnsubscribeAction _unsubscribe;

		public MessageTraceBusService(UntypedChannel eventChannel)
		{
			_eventChannel = eventChannel;
			_fiber = new PoolFiber();
			_messageList = new LinkedList<MessageTraceDetail>();
			_detailLimit = 100;

			_connection = _eventChannel.Connect(x =>
				{
					x.AddConsumerOf<MessageReceived>()
						.UsingConsumer(Handle)
						.HandleOnFiber(_fiber);
				});
		}

		public void Consume(GetMessageTraceList message)
		{
			IConsumeContext<GetMessageTraceList> context = ContextStorage.MessageContext<GetMessageTraceList>();

			Uri responseAddress = context.ResponseAddress;
			if (responseAddress == null)
				return;

			int count = message.Count;

			IEndpoint responseEndpoint = context.Bus.GetEndpoint(responseAddress);

			_fiber.Add(() => SendLastTraceMessagesTo(responseEndpoint, count));
		}

		public void Dispose()
		{
			_connection.Dispose();
		}

		public void Start(IServiceBus bus)
		{
			_controlBus = bus.ControlBus;
			_unsubscribe = _controlBus.SubscribeInstance(this);
		}

		public void Stop()
		{
			_unsubscribe();
			_connection.Disconnect();
		}

		void Handle(MessageReceived message)
		{
			MessageTraceDetail detail = new MessageTraceDetailImpl
				{
					Id = message.Id,
					MessageId = message.Context.MessageId,
					MessageType = message.Context.MessageType,
					ContentType = message.Context.ContentType,
					SourceAddress = message.Context.SourceAddress,
					InputAddress = message.Context.InputAddress,
					DestinationAddress = message.Context.DestinationAddress,
					ResponseAddress = message.Context.ResponseAddress,
					FaultAddress = message.Context.FaultAddress,
					Network = message.Context.Network,
					ExpirationTime = message.Context.ExpirationTime,
					RetryCount = message.Context.RetryCount,
					ReceivedAt = message.ReceivedAt,
					Duration = message.ReceiveDuration,
				};

			var node = new LinkedListNode<MessageTraceDetail>(detail);

			_messageList.AddLast(node);

			RemoveExtraDetailFromList();
		}

		void RemoveExtraDetailFromList()
		{
			int removeCount = _messageList.Count - _detailLimit;
			if (removeCount > 0)
			{
				for (int i = 0; i < removeCount; i++)
				{
					_messageList.RemoveFirst();
				}
			}
		}

		void SendLastTraceMessagesTo(IEndpoint endpoint, int count)
		{
			try
			{
				IList<MessageTraceDetail> details = _messageList.Reverse().Take(count).ToList();

				var message = new MessageTraceListImpl {Messages = details};

				endpoint.Send<MessageTraceList>(message);
			}
			catch (Exception ex)
			{
				_log.Error("Failed to send message", ex);
			}
		}
	}
}