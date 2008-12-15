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
namespace MassTransit.Transports
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using log4net;
	using Magnum.Common.Threading;
	using Serialization;

	public class LoopbackEndpoint : 
		IEndpoint
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (LoopbackEndpoint));
		private static readonly ILog _messageLog = LogManager.GetLogger("MassTransit.Messages");
		private readonly UpgradeableLock _lockContext = new UpgradeableLock();

		private readonly Semaphore _messageReady = new Semaphore(0, int.MaxValue);
		private readonly Queue<byte[]> _messages = new Queue<byte[]>();
		private readonly IMessageSerializer _serializer = new BinaryMessageSerializer();
		private readonly Uri _uri;
		private bool _disposed;

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
			if (_disposed) throw new ObjectDisposedException("The object has been disposed");

			if (_messageLog.IsInfoEnabled)
				_messageLog.InfoFormat("SEND:{0}:{1}", Uri, typeof (T).Name);

			Enqueue(message);
		}

		public void Send<T>(T message, TimeSpan timeToLive) where T : class
		{
			if (_disposed) throw new ObjectDisposedException("The object has been disposed");

			if (_messageLog.IsInfoEnabled)
				_messageLog.InfoFormat("SEND:{0}:{1}", Uri, typeof (T).Name);

			Enqueue(message);
		}

		public void Receive(TimeSpan timeout, Func<object, Func<object, bool>, bool> receiver)
		{
			if (_disposed) throw new ObjectDisposedException("The object has been disposed");

			if (!_messageReady.WaitOne(timeout, true))
				return;

			try
			{
				object obj = Dequeue();

				if (receiver(obj, x =>
					{
						if (_messageLog.IsInfoEnabled)
							_messageLog.InfoFormat("RECV:{0}:{1}", _uri, obj.GetType().Name);

						return true;
					}))
					return;

				if (_messageLog.IsInfoEnabled)
					_messageLog.InfoFormat("SKIP:{0}:{1}", _uri, obj.GetType().Name);
			}
			catch (InvalidOperationException)
			{
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				using (_lockContext.EnterWriteLock())
					_messages.Clear();
			}
			_disposed = true;
		}

		private void Enqueue<T>(T message)
		{
			using (MemoryStream mstream = new MemoryStream())
			{
				_serializer.Serialize(mstream, message);
				using (_lockContext.EnterWriteLock())
					_messages.Enqueue(mstream.ToArray());
			}

			_messageReady.Release();
		}

		private object Dequeue()
		{
			byte[] buffer;
			using (_lockContext.EnterWriteLock())
				buffer = _messages.Dequeue();

			using (MemoryStream mstream = new MemoryStream(buffer))
			{
				object obj = _serializer.Deserialize(mstream);

				return obj;
			}
		}
	}
}