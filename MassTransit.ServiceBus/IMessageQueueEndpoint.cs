using System.Messaging;

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
	/// <summary>
	/// An extension of the IEndpoint interface for the additional support of Message Queue backed endpoints
	/// </summary>
    public interface IMessageQueueEndpoint :
        IEndpoint
    {
        /// <summary>
        /// The path of the message queue for the endpoint. Suitable for use with <c ref="MessageQueue" />.Open
        /// to access a message queue.
        /// </summary>
        string QueuePath { get; }

        /// <summary>
        /// Opens a message queue
        /// </summary>
        /// <param name="mode">The access mode for the queue</param>
        /// <returns>An open <c ref="MessageQueue" /> object</returns>
	    MessageQueue Open(QueueAccessMode mode);
    }
}