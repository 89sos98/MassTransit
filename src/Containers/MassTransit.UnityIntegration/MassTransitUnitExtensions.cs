// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.UnityIntegration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using BusConfigurators;
	using Magnum.Extensions;
	using Microsoft.Practices.Unity;

	public static class MassTransitUnitExtensions
	{
		public static void LoadConsumersFromContainer(this ServiceBusConfigurator configurator, UnityContainer container)
		{
			IList<Type> concreteTypes = container.Registrations
				.Where(r => r.MappedToType.Implements<IConsumer>())
				.Select(r => r.RegisteredType)
				.ToList();

			if (concreteTypes.Count == 0)
				return;

			configurator.RegisterSubscription(
				x => concreteTypes.Each(type => x.RegisterConsumer(type, y => container.Resolve(y))));
		}
	}
}