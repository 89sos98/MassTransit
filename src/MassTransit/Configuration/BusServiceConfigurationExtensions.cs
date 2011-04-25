﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit
{
	using System;
	using BusConfigurators;
	using BusServiceConfigurators;
	using Configurators;
	using Internal;

	public static class BusServiceConfigurationExtensions
	{
		public static void ConfigureService<TServiceConfigurator>(this ServiceBusConfigurator configurator, 
			Action<TServiceConfigurator> configure)
			where TServiceConfigurator : BusServiceConfigurator, new()
		{
			var serviceConfigurator = new TServiceConfigurator();

			configure(serviceConfigurator);

			var busConfigurator = new CustomBusServiceConfigurator(serviceConfigurator);

			configurator.AddBusConfigurator(busConfigurator);
		}

		public static void AddService<TService>(this ServiceBusConfigurator configurator)
			where TService : IBusService, new()
		{
			var serviceConfigurator = new DefaultBusServiceConfigurator<TService>(bus => new TService());

			configurator.AddBusConfigurator(serviceConfigurator);
		}

		public static void AddService<TService>(this ServiceBusConfigurator configurator, Func<TService> serviceFactory)
			where TService : IBusService
		{
			var serviceConfigurator = new DefaultBusServiceConfigurator<TService>(bus => serviceFactory());

			configurator.AddBusConfigurator(serviceConfigurator);
		}

		public static void AddService<TService>(this ServiceBusConfigurator configurator, Func<IServiceBus, TService> serviceFactory)
			where TService : IBusService
		{
			var serviceConfigurator = new DefaultBusServiceConfigurator<TService>(serviceFactory);

			configurator.AddBusConfigurator(serviceConfigurator);
		}
	}
}