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
namespace MassTransit.Pipeline
{
	using System;

	public interface IConfigureInboundPipeline
	{
		/// <summary>
		/// Configure the MessagePipeline
		/// </summary>
		/// <typeparam name="V"></typeparam>
		/// <param name="action"></param>
		/// <returns></returns>
		V Configure<V>(Func<IConfigureInboundPipeline, V> action);

		Func<bool> Subscribe<TComponent>()
			where TComponent : class;

		Func<bool> Subscribe<TComponent>(TComponent instance)
			where TComponent : class;
	}
}