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
namespace MassTransit.Tests.Serialization
{
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using MassTransit.Serialization;
	using MassTransit.Serialization.Custom;
	using NUnit.Framework;

	public class SerializationSpecificationBase
	{
		protected void TestSerialization<T>(T message)
		{
			byte[] data;
			var serializer = new CustomXmlSerializer();

			using (MemoryStream output = new MemoryStream())
			{
				serializer.Serialize(output, message);

				data = output.ToArray();
			}

			Trace.WriteLine(Encoding.UTF8.GetString(data));

			using (MemoryStream input = new MemoryStream(data))
			{
				object receivedMessage = serializer.Deserialize(input);

				Assert.AreEqual(message, receivedMessage);
				Assert.AreNotSame(message, receivedMessage);
			}
		}
	}
}