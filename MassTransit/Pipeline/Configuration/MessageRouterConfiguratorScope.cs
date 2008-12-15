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
namespace MassTransit.Pipeline.Configuration
{
	using Inspectors;
	using Sinks;

	public class MessageRouterConfiguratorScope<TMessage> :
		PipelineInspectorBase<MessageRouterConfiguratorScope<TMessage>>
		where TMessage : class
	{
		public MessageRouter<object> ObjectRouter { get; private set; }
		public MessageRouter<TMessage> Router { get; private set; }

		protected bool Inspect<TRoutedMessage>(MessageRouter<TRoutedMessage> element)
			where TRoutedMessage : class
		{
			if (typeof (TRoutedMessage) == typeof (TMessage))
			{
				Router = TranslateTo<MessageRouter<TMessage>>.From(element);

				return false;
			}

			if (typeof (TRoutedMessage) == typeof (object))
			{
				ObjectRouter = TranslateTo<MessageRouter<object>>.From(element);
			}

			return true;
		}
	}
}