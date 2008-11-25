namespace MassTransit.SQS
{
	using System;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using Amazon.SQS;
	using Amazon.SQS.Model;
	using Exceptions;
	using Internal;
	using log4net;

	public class AmazonEndpoint :
		IEndpoint
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (AmazonEndpoint));
		private static readonly ILog _messageLog = SpecialLoggers.Messages;

		private readonly BinaryFormatter _formatter = new BinaryFormatter();
		private readonly Uri _uri;
		private AmazonSQS _service;
		private readonly string _queueName;

		public AmazonEndpoint(Uri uri)
		{
			_uri = uri;
			_queueName = Uri.AbsolutePath.Substring(1);

			string userInfo = _uri.UserInfo;

			string[] segments = userInfo.Split(':');

			string accessKeyId = segments[0];
			string secretAccessKey = segments[1];

			_service = new AmazonSQSClient(accessKeyId, secretAccessKey);

			ListQueues request = new ListQueues()
				.WithQueueNamePrefix(_queueName);

			ListQueuesResponse response = _service.ListQueues(request);
			if(response.ListQueuesResult.QueueUrl.Count == 0)
			{
				CreateQueue createQueue = new CreateQueue()
					.WithQueueName(_queueName);

				_service.CreateQueue(createQueue);
			}
		}

		public static string Scheme
		{
			get { return "amazon"; }
		}

		public Uri Uri
		{
			get { return _uri; }
		}

		public void Send<T>(T message) where T : class
		{
			Send(message, TimeSpan.MaxValue);
		}

		public void Send<T>(T message, TimeSpan timeToLive) where T : class
		{
			try
			{
				string body;
				using(MemoryStream mstream = new MemoryStream())
				{
					_formatter.Serialize(mstream, message);

					body = Convert.ToBase64String(mstream.ToArray());
				}

				SendMessage request = new SendMessage()
					.WithQueueName(_queueName)
					.WithMessageBody(body);

				SendMessageResponse response = _service.SendMessage(request);

				_messageLog.DebugFormat("SENT: {0} - {1}", _queueName, response.SendMessageResult.MessageId);
				
			}
			catch (Exception ex)
			{
				throw new EndpointException(this, "Error sending message to " + _queueName, ex);
			}
		}

		public object Receive(TimeSpan timeout)
		{
			return Receive(timeout, delegate { return true; });
		}

		public object Receive(TimeSpan timeout, Predicate<object> accept)
		{
			try
			{
				ReceiveMessage request = new ReceiveMessage()
					.WithQueueName(_queueName)
					.WithVisibilityTimeout((int) timeout.TotalSeconds)
					.WithMaxNumberOfMessages(1m);

				ReceiveMessageResponse response = _service.ReceiveMessage(request);

				foreach (Message message in response.ReceiveMessageResult.Message)
				{
					byte[] body = Convert.FromBase64String(message.Body);
					using(MemoryStream mstream = new MemoryStream(body, false))
					{
						object msg = _formatter.Deserialize(mstream);

						if(accept(msg))
						{
							DeleteMessage deleteRequest = new DeleteMessage()
								.WithQueueName(_queueName)
								.WithReceiptHandle(message.ReceiptHandle);

							_service.DeleteMessage(deleteRequest);

							_messageLog.DebugFormat("RECV: {0} - {1}", _queueName, message.MessageId);

							return msg;
						}
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new EndpointException(this, "Error receiving message from " + _queueName, ex);
			}
		}

		public void Dispose()
		{
			_service = null;
		}
	}
}