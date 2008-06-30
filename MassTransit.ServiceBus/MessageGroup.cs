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
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Util;

	[Serializable]
	public class MessageGroup
	{
		private static readonly MethodInfo _publishMethodInfo = typeof (IServiceBus).GetMethod("Publish", BindingFlags.Public | BindingFlags.Instance);
		private readonly List<object> _messages;

		public MessageGroup(List<object> messages)
		{
			_messages = messages;
		}

		public int Count
		{
			get { return _messages.Count; }
		}

		public object this[int index]
		{
			get { return _messages[index]; }
		}

		public static MessageGroupBuilder<TGroup> Build<TGroup>() where TGroup : class
		{
			return new MessageGroupBuilder<TGroup>();
		}

		public static MessageGroup Join(params object[] items)
		{
			List<object> messages = new List<object>(items);

			return new MessageGroup(messages);
		}

		public object[] ToArray()
		{
			return _messages.ToArray();
		}

		public T Get<T>(int index)
		{
			Guard.Against.IndexOutOfRange(index, _messages.Count);

			Type typeofT = typeof (T);

			object obj = _messages[index];

			Type objType = obj.GetType();

			if (typeofT.IsAssignableFrom(objType))
				return (T) obj;

			throw new ArgumentException(string.Format("The message at the specified index could not be converted to the type specified ({0} -> {1})", objType, typeofT));
		}

		public void Split(IServiceBus bus)
		{
			foreach (object message in _messages)
			{
				RepublishMessage(message, bus);
			}
		}

		private static void RepublishMessage(object message, IServiceBus bus)
		{
			Type objType = message.GetType();

			if (!objType.IsSerializable)
			{
				//_log.ErrorFormat("")
			}

			MethodInfo inv = GetPublishMethod(objType);

			inv.Invoke(bus, new object[] {message});
		}

		private static MethodInfo GetPublishMethod(Type objType)
		{
			return _publishMethodInfo.MakeGenericMethod(objType);
		}
	}

	public class MessageGroupBuilder<TBuilder> where TBuilder : class
	{
		internal readonly List<object> _messages = new List<object>();
		internal static readonly AllowMessageType allow;

		static MessageGroupBuilder()
		{
			object[] attributes = typeof (TBuilder).GetCustomAttributes(typeof (AllowMessageType), false);
			if(attributes != null)
			{
				allow = attributes[0] as AllowMessageType;
			}
		}

		public MessageGroupBuilder<TBuilder> Add<T>(T message) where T : class
		{
			Type typeofT = typeof (T);

			if (MessageTypeAllowed(typeofT))
				_messages.Add(message);
			else
			{
				throw new ArgumentException(typeofT.FullName + " is not an allowed message type for this group", "message");
			}

			return this;
		}

		private static bool MessageTypeAllowed(Type t)
		{
			if (allow == null)
				return true;

			return allow.GetUsage(t) != MessageTypeUsage.None;
		}

		public static implicit operator TBuilder(MessageGroupBuilder<TBuilder> builder)
		{
			return Activator.CreateInstance(typeof (TBuilder), builder._messages) as TBuilder;
		}
	}
}