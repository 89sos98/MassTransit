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
namespace MassTransit.Saga.Configuration
{
	using System;
	using System.Collections.Generic;
	using Magnum;
	using Magnum.Reflection;
	using MassTransit.Pipeline;
	using MassTransit.Pipeline.Configuration.Subscribers;

	public class SagaStateMachineSubscriber :
		PipelineSubscriberBase
	{
		public override IEnumerable<UnsubscribeAction> Subscribe<TComponent>(ISubscriberContext context)
		{
			if (!IsSagaStateMachine<TComponent>())
				yield break;

			var results = this.Call<IEnumerable<UnsubscribeAction>>("Connect", new[] {typeof (TComponent)}, context);
			foreach (var unsubscribeAction in results)
			{
				yield return unsubscribeAction;
			}
		}

		public override IEnumerable<UnsubscribeAction> Subscribe<TComponent>(ISubscriberContext context, TComponent instance)
		{
			yield break;
		}

		protected virtual IEnumerable<UnsubscribeAction> Connect<TComponent>(ISubscriberContext context)
			where TComponent : SagaStateMachine<TComponent>, ISaga
		{
			var component = (TComponent) Activator.CreateInstance(typeof (TComponent), CombGuid.Generate());

			var inspector = new SagaStateMachineEventInspector<TComponent>();
			component.Inspect(inspector);

			var subscriber = new SagaEventSubscriber<TComponent>(context, new SagaPolicyFactory());

			foreach (var result in inspector.GetResults())
			{
				yield return subscriber.Connect(result.SagaEvent.MessageType, result.SagaEvent.Event, result.States);
			}
		}

		private static bool IsSagaStateMachine<TComponent>()
		{
			Type componentType = typeof (TComponent);

			while (componentType.BaseType != null)
			{
				Type baseType = componentType.BaseType;
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof (SagaStateMachine<>))
					return true;

				componentType = baseType;
			}

			return false;
		}
	}
}