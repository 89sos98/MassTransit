/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.

namespace MassTransit.ServiceBus
{
    using System.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Util;

    public class EnvelopeMessageMapper
    {
        private readonly static IFormatter _formatter = new BinaryFormatter();

        public static IEnvelope MapFrom(Message msg)
        {
            IMessageQueueEndpoint returnAddress = (msg.ResponseQueue != null) ? MessageQueueEndpoint.FromQueuePath(msg.ResponseQueue.Path) : null;
                
            IEnvelope e = new Envelope(returnAddress);

            if(string.IsNullOrEmpty(msg.Id))
            {
                e.Id = MessageId.Empty;
            }
            else
            {
                e.Id = msg.Id;
            }

            if(string.IsNullOrEmpty(msg.CorrelationId))
            {
                e.CorrelationId = MessageId.Empty;
            }
            else
            {
                e.CorrelationId = msg.CorrelationId;
            }

            e.TimeToBeReceived = msg.TimeToBeReceived;
            e.Recoverable = msg.Recoverable;
            e.Label = msg.Label;

            if (e.Id != MessageId.Empty)
            {
                e.SentTime = msg.SentTime;
                e.ArrivedTime = msg.ArrivedTime;
            }

            IMessage[] messages = _formatter.Deserialize(msg.BodyStream) as IMessage[];
            
            e.Messages = messages ?? new IMessage[] {};

            return e;
        }

        public static Message MapFrom(IEnvelope envelope)
        {
            Message msg = new Message();

            if (envelope.Messages != null && envelope.Messages.Length > 0)
            {
                _formatter.Serialize(msg.BodyStream, envelope.Messages);
            }

        	IMessageQueueEndpoint endpoint = envelope.ReturnEndpoint as IMessageQueueEndpoint;

			if(endpoint != null)
				msg.ResponseQueue = new MessageQueue(endpoint.QueueName);

            if (envelope.TimeToBeReceived < MessageQueue.InfiniteTimeout)
                msg.TimeToBeReceived = envelope.TimeToBeReceived;

            if (!string.IsNullOrEmpty(envelope.Label))
                msg.Label = envelope.Label;

            msg.Recoverable = envelope.Recoverable;

            if (envelope.CorrelationId != MessageId.Empty)
                msg.CorrelationId = envelope.CorrelationId;

            return msg;
        }
    }
}