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
	using System.Collections;

    public interface IObjectBuilder
	{
		/// <summary>
		/// Build an object of the specified type
		/// </summary>
		/// <param name="objectType">The type of object to build</param>
		/// <returns>The object that was built</returns>
		object Build(Type objectType);

		/// <summary>
		/// Build an object of the specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Build<T>() where T : class;


        T Build<T>(Type component) where T : class;

	    T Build<T>(IDictionary arguments);

		/// <summary>
		/// Releases an object back to the container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		void Release<T>(T obj);
	}
}