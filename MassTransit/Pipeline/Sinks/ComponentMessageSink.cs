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
namespace MassTransit.Pipeline.Sinks
{
	using System.Collections.Generic;
	using Exceptions;
	using Interceptors;

	/// <summary>
	/// Routes messages to instances of subscribed components. A new instance of the component
	/// is created from the container for each message received.
	/// </summary>
	/// <typeparam name="TComponent">The component type to handle the message</typeparam>
	/// <typeparam name="TMessage">The message to handle</typeparam>
	public class ComponentMessageSink<TComponent, TMessage> :
		IMessageSink<TMessage>
		where TMessage : class
		where TComponent : class, Consumes<TMessage>.All
	{
		private readonly IObjectBuilder _builder;

		public ComponentMessageSink(IInterceptorContext context)
		{
			_builder = context.Builder;
		}

		public void Dispose()
		{
		}

		public IEnumerable<Consumes<TMessage>.All> Enumerate(TMessage message)
		{
			Consumes<TMessage>.All consumer = BuildConsumer();

			try
			{
				yield return consumer;
			}
			finally
			{
				Release(consumer);
			}
		}

		public bool Inspect(IPipelineInspector inspector)
		{
			return inspector.Inspect(this);
		}

		private void Release(Consumes<TMessage>.All consumer)
		{
			_builder.Release(TranslateTo<TComponent>.From(consumer));
		}

		private Consumes<TMessage>.All BuildConsumer()
		{
			TComponent component = _builder.GetInstance<TComponent>();
			if (component == null)
				throw new ConfigurationException("Unable to resolve type from container: " + typeof (TComponent));

			Consumes<TMessage>.All consumer = TranslateTo<Consumes<TMessage>.All>.From(component);

			return consumer;
		}
	}
}