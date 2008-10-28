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
namespace MassTransit.ServiceBus.Services.MessageDeferral.Messages
{
    using System;
    using Util;

    [Serializable]
    public class DeferMessage
    {
        private Guid _correlationId;
        private DateTime _deliverAt;
        private object _message;
        private string _messageType;

        protected DeferMessage()
        {
        }

        public DeferMessage(Guid correlationId, TimeSpan deliverIn, object message)
            : this(correlationId, DateTime.UtcNow + deliverIn, message)
        {
        }

        public DeferMessage(Guid correlationId, DateTime deliverAt, object message)
        {
            Guard.Against.Null(message, "Message must not be null");

            _correlationId = correlationId;
            _message = message;
            _deliverAt = deliverAt.ToUniversalTime();

            _messageType = message.GetType().AssemblyQualifiedName;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
            set { _correlationId = value; }
        }

        public DateTime DeliverAt
        {
            get { return _deliverAt; }
            set { _deliverAt = value; }
        }

        public object Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string MessageType
        {
            get { return _messageType; }
            set { _messageType = value; }
        }
    }
}