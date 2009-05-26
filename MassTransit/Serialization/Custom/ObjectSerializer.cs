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
	using System.Collections.Generic;
	using System.Xml;
	using Magnum.Monads;

	public class ObjectSerializer<T> :
		IObjectSerializer
	{
		private readonly IObjectPropertyCache<T> _propertyCache;
		private readonly IObjectFieldCache _fieldCache;
		private readonly Type _type;
		private readonly string _ns;

		public ObjectSerializer(IObjectFieldCache fieldCache)
		{
			_propertyCache = new ObjectPropertyCache<T>();
			_fieldCache = fieldCache;

			_type = typeof(T);
			_ns = _type.ToMessageName();
		}

		public IEnumerable<K<Action<XmlWriter>>> GetSerializationActions(ISerializerContext context, string localName, object value)
		{
			if(value == null)
				yield break;

			string prefix = context.GetPrefix(localName, _ns);

			yield return output => output(writer =>
				{
					bool isDocumentElement = writer.WriteState == WriteState.Start;

					writer.WriteStartElement(prefix, localName, _ns);

					if (isDocumentElement)
						context.WriteNamespaceInformationToXml(writer);
				});

			foreach (ObjectProperty<T> property in _propertyCache.GetProperties())
			{
				object obj = property.GetValue((T)value);

				var enumerable = context.SerializeObject(property.Name, property.PropertyType, obj);
				foreach (var action in enumerable)
				{
					yield return action;
				}
			}

			yield return output => output(writer => { writer.WriteEndElement(); });
		}
	}
}