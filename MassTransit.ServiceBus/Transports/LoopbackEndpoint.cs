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
namespace MassTransit.ServiceBus.Transports
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading;
	using log4net;

	public class LoopbackEndpoint : IEndpoint
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (LoopbackEndpoint));
		private static readonly ILog _messageLog = LogManager.GetLogger("MassTransit.Messages");

		private readonly BinaryFormatter _formatter = new BinaryFormatter();
		private readonly Semaphore _messageReady = new Semaphore(0, int.MaxValue);
		private readonly Queue<byte[]> _messages = new Queue<byte[]>();
		private readonly Uri _uri;

		public LoopbackEndpoint(Uri uri)
		{
			_uri = uri;
		}

		public static string Scheme
		{
			get { return "loopback"; }
		}

		public Uri Uri
		{
			get { return _uri; }
		}

		public void Send<T>(T message) where T : class
		{
			Enqueue(message);
		}

		public void Send<T>(T message, TimeSpan timeToLive) where T : class
		{
			Enqueue(message);
		}

		public object Receive()
		{
			return Receive(TimeSpan.MaxValue);
		}

		public object Receive(TimeSpan timeout)
		{
			if (_messageReady.WaitOne(timeout, true))
			{
				object obj =  Dequeue();

				if (_messageLog.IsInfoEnabled)
					_messageLog.InfoFormat("RECV:{0}:{1}", _uri, obj.GetType().Name);

				return obj;
			}

			return null;
		}

		public object Receive(Predicate<object> accept)
		{
			return Receive(TimeSpan.MaxValue, accept);
		}

		public object Receive(TimeSpan timeout, Predicate<object> accept)
		{
			if (_messageReady.WaitOne(timeout, true))
			{
				object obj = Dequeue();

				if (accept(obj))
				{
					if (_messageLog.IsInfoEnabled)
						_messageLog.InfoFormat("RECV:{0}:{1}", _uri, obj.GetType().Name);

					return obj;
				}
			}

			return null;
		}

		public T Receive<T>() where T : class
		{
			return Receive<T>(TimeSpan.MaxValue);
		}

		public T Receive<T>(TimeSpan timeout) where T : class
		{
			try
			{
				return (T) Receive(timeout, delegate(object obj)
				                            	{
				                            		Type messageType = obj.GetType();

				                            		if (messageType != typeof (T))
				                            			return false;

				                            		return true;
				                            	});
			}
			catch (Exception ex)
			{
				string message = string.Format("Error on receive with Receive<{0}> accept", typeof (T).Name);
				_log.Error(message, ex);
			}

			throw new Exception("Receive<T>(TimeSpan timeout) didn't error");
		}

		public T Receive<T>(Predicate<T> accept) where T : class
		{
			return Receive<T>(TimeSpan.MaxValue, accept);
		}

		public T Receive<T>(TimeSpan timeout, Predicate<T> accept) where T : class
		{
			try
			{
				return (T) Receive(timeout, delegate(object obj)
				                            	{
				                            		Type messageType = obj.GetType();

				                            		if (messageType != typeof (T))
				                            			return false;

				                            		T message = obj as T;
				                            		if (message == null)
				                            			return false;

				                            		return accept(message);
				                            	});
			}
			catch (Exception ex)
			{
				string message = string.Format("Error on receive with Predicate<{0}> accept", typeof (T).Name);
				_log.Error(message, ex);
			}

			throw new Exception("Receive<T>(TimeSpan timeout, Predicate<T> accept) had a weird error");
		}

		public void Dispose()
		{
			lock(_messages)
				_messages.Clear();
		}

		private void Enqueue<T>(T message)
		{
			using (MemoryStream mstream = new MemoryStream())
			{
				_formatter.Serialize(mstream, message);
				lock(_messages)
					_messages.Enqueue(mstream.ToArray());
			}

			_messageReady.Release();
		}

		private object Dequeue()
		{
			byte[] buffer;
			lock(_messages)
				buffer = _messages.Dequeue();

			using (MemoryStream mstream = new MemoryStream(buffer))
			{
				object obj = _formatter.Deserialize(mstream);

				return obj;
			}
		}
	}
}