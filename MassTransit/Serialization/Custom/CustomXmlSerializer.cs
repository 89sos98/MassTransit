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
namespace MassTransit.Serialization.Custom
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Xml;
	using Internal;
	using Magnum.Monads;

	public class CustomXmlSerializer :
		IMessageSerializer
	{
		private XmlReaderSettings _readerSettings;
		private XmlWriterSettings _writerSettings;

		public CustomXmlSerializer()
		{
			_writerSettings = new XmlWriterSettings
				{
					Encoding = Encoding.UTF8,
					Indent = true,
					NewLineOnAttributes = true,
				};

			_readerSettings = new XmlReaderSettings
				{
					IgnoreWhitespace = true
				};
		}

		public void Serialize<T>(Stream stream, T message)
		{
			using (var streamWriter = new StreamWriter(stream))
			using (var writer = XmlWriter.Create(streamWriter, _writerSettings))
			{
				SerializerContext context = new SerializerContext();

				foreach (K<Action<XmlWriter>> writerAction in context.Serialize(message).ToArray())
				{
					writerAction(x => x(writer));
				}

				writer.Close();
				streamWriter.Close();
			}
		}

		public object Deserialize(Stream input)
		{
			using (StreamReader streamReader = new StreamReader(input))
			using (XmlReader reader = XmlReader.Create(streamReader, _readerSettings))
			{
				IDeserializerContext context = new DeserializerContext(reader);

				object message = context.Deserialize();

				if (message == null)
					throw new SerializationException("Could not deserialize message.");

				if (message is XmlMessageEnvelope)
				{
					XmlMessageEnvelope envelope = message as XmlMessageEnvelope;

					InboundMessageHeaders.SetCurrent(envelope.GetMessageHeadersSetAction());

					return envelope.Message;
				}

				return message;
			}
		}

		public byte[] Serialize<T>(T message)
		{
			using (MemoryStream output = new MemoryStream())
			{
				Serialize(output, message);

				return output.ToArray();
			}
		}

		public object Deserialize(byte[] data)
		{
			using(MemoryStream input = new MemoryStream(data))
			{
				return Deserialize(input);
			}
		}
	}
}