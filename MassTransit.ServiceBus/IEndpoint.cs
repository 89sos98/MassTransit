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

using System;

namespace MassTransit.ServiceBus
{
	/// <summary>
	/// IEndpoint is implemented by an endpoint. An endpoint is an addressable location on the network.
	/// </summary>
    public interface IEndpoint :
        IDisposable
    {
		/// <summary>
		/// The address of the endpoint, in URI format
		/// </summary>
		Uri Uri { get; }

        /// <summary>
        /// Returns an interface to send messages on this endpoint
        /// </summary>
        IMessageSender Sender { get; }

        /// <summary>
        /// Returns an interface to receive messages on this endpoint
        /// </summary>
        IMessageReceiver Receiver { get; }
    }
}