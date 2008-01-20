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
using System.Messaging;
using MassTransit.ServiceBus.Exceptions;

namespace MassTransit.ServiceBus
{
	/// <summary>
	/// A MessageQueueEndpoint is an implementation of an endpoint using the Microsoft Message Queue service.
	/// </summary>
	public class MessageQueueEndpoint : IMessageQueueEndpoint
	{
		private readonly string _queuePath;
		private readonly Uri _uri;

		//private MessageQueue _queue;

		/// <summary>
		/// Initializes a <c ref="MessageQueueEndpoint" /> instance with the specified URI string.
		/// </summary>
		/// <param name="uriString">The URI for the endpoint</param>
		public MessageQueueEndpoint(string uriString)
			: this(new Uri(uriString))
		{
		}

		/// <summary>
		/// Initializes a <c ref="MessageQueueEndpoint" /> instance with the specified URI.
		/// </summary>
		/// <param name="uri">The URI for the endpoint</param>
		public MessageQueueEndpoint(Uri uri)
		{
			_uri = uri;

			if (_uri.AbsolutePath.IndexOf("/", 1) >= 0)
			{
				throw new EndpointException(this, "Queue Endpoints can't have a child folder unless it is 'public'. Good: 'msmq://machinename/queue_name' or 'msmq://machinename/public/queue_name' - Bad: msmq://machinename/queue_name/bad_form");
			}

            string hostName = _uri.Host;
            if (string.Compare(hostName, ".") == 0 || string.Compare(hostName, "localhost", true) == 0)
            {
                hostName = Environment.MachineName.ToLowerInvariant();
            }
            
            if (string.Compare(_uri.Host, "localhost", true) == 0)
            {
                _uri = new Uri("msmq://" + Environment.MachineName.ToLowerInvariant() + _uri.AbsolutePath);
            }

            _queuePath = string.Format(@"FormatName:DIRECT=OS:{0}\private$\{1}", hostName, _uri.AbsolutePath.Substring(1));
		}

		#region IMessageQueueEndpoint Members

	    /// <summary>
	    /// The path of the message queue for the endpoint. Suitable for use with <c ref="MessageQueue" />.Open
	    /// to access a message queue.
	    /// </summary>
	    public string QueueName
		{
            get { return _queuePath; }
		}

        public MessageQueue Open(QueueAccessMode mode)
        {
            MessageQueue queue = new MessageQueue(QueueName, mode);

            MessagePropertyFilter mpf = new MessagePropertyFilter();
            mpf.SetAll();

            queue.MessageReadPropertyFilter = mpf;

            return queue;
        }

	    /// <summary>
	    /// The address of the endpoint, in URI format
	    /// </summary>
	    public Uri Uri
		{
			get { return _uri; }
		}

	    ///<summary>
	    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	    ///</summary>
	    ///<filterpriority>2</filterpriority>
	    public void Dispose()
		{
		}

		#endregion

		//private MessageQueueTransactionType GetTransactionType()
		//{
		//    MessageQueueTransactionType tt = MessageQueueTransactionType.None;
		//    if (_queue.Transactional)
		//    {
		//        Check.RequireTransaction(
		//            string.Format(
		//                "The current queue {0} is transactional and this MessageQueueEndpoint is not running in a transaction.",
		//                _uri));

		//        tt = MessageQueueTransactionType.Automatic;
		//    }
		//    return tt;
		//}

		/// <summary>
		/// Implicitly creates a <c ref="MessageQueueEndpoint" />.
		/// </summary>
		/// <param name="queueUri">A string identifying the URI of the message queue (ex. msmq://localhost/my_queue)</param>
		/// <returns>An instance of the MessageQueueEndpoint class</returns>
		public static implicit operator MessageQueueEndpoint(string queueUri)
		{
			return new MessageQueueEndpoint(queueUri);
		}

		/// <summary>
		/// Returns the URI string for the message queue endpoint.
		/// </summary>
		/// <param name="endpoint">The endpoint to use to generate the URI string</param>
		/// <returns>A URI string that identifies the message queue endpoint</returns>
		public static implicit operator string(MessageQueueEndpoint endpoint)
		{
			return endpoint.Uri.AbsoluteUri;
		}

		/// <summary>
		/// Creates an instance of the <c ref="MessageQueueEndpoint" /> class using the specified queue path
		/// </summary>
		/// <param name="path">A path to a Microsoft Message Queue</param>
		/// <returns>An instance of the <c ref="MessageQueueEndpoint" /> class for the specified queue</returns>
		public static IMessageQueueEndpoint FromQueuePath(string path)
		{
            //TODO: Lots of duplicated logic here? -d

			const string prefix = "FormatName:DIRECT=OS:";

			if (path.Length > prefix.Length && path.Substring(0, prefix.Length).ToUpperInvariant() == prefix.ToUpperInvariant())
				path = path.Substring(prefix.Length);

			string[] parts = path.Split('\\');

			if (parts.Length != 3)
				throw new ArgumentException("Invalid Queue Path Specified");

            //Validate parts[1] = private$
			if (string.Compare(parts[1], "private$", true) != 0)
				throw new ArgumentException("Invalid Queue Path Specified");

            if (parts[0] == ".")
                parts[0] = Environment.MachineName.ToLowerInvariant();

			return new MessageQueueEndpoint(string.Format("msmq://{0}/{1}", parts[0], parts[2]));
		}
	}
}