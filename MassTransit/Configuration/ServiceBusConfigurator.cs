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
namespace MassTransit.Configuration
{
	using System;
	using System.Collections.Generic;
	using Exceptions;
	using Internal;
	using Subscriptions;

	public class ServiceBusConfigurator :
		ServiceBusConfiguratorDefaults,
		IServiceBusConfigurator
	{
		private static readonly ServiceBusConfiguratorDefaults _defaults = new ServiceBusConfiguratorDefaults();
		private readonly List<Action<IServiceBus, ISubscriptionCache, IObjectBuilder, Action<Type, IBusService>>> _services;
		private Uri _receiveFromUri;

		private ServiceBusConfigurator()
		{
			_services = new List<Action<IServiceBus, ISubscriptionCache, IObjectBuilder, Action<Type, IBusService>>>();

			_defaults.ApplyTo(this);
		}

		public void ReceiveFrom(string uriString)
		{
			try
			{
				_receiveFromUri = new Uri(uriString);
			}
			catch (UriFormatException ex)
			{
				throw new ConfigurationException("The Uri for the receive endpoint is invalid: " + uriString, ex);
			}
		}

		public void ReceiveFrom(Uri uri)
		{
			_receiveFromUri = uri;
		}

		public void ConfigureService<TServiceConfigurator>(Action<TServiceConfigurator> configure)
			where TServiceConfigurator : IServiceConfigurator, new()
		{
			_services.Add((bus, cache, builder, add) =>
				{
					TServiceConfigurator configurator = new TServiceConfigurator();

					configure(configurator);

					var service = configurator.Create(bus, cache, builder);

					add(configurator.ServiceType, service);
				});
		}

		private IServiceBus Create()
		{
			ServiceBus bus = CreateServiceBus();

			ConfigurePoisonEndpoint(bus);

			ConfigureThreadLimits(bus);

			if (AutoSubscribe)
			{
				// get all the types and subscribe them to the bus
			}

			ConfigureBusServices(bus);

			if (AutoStart)
			{
				bus.Start();
			}

			return bus;
		}

		private void ConfigurePoisonEndpoint(ServiceBus bus)
		{
			if (ErrorUri != null)
			{
				bus.PoisonEndpoint = bus.EndpointFactory.GetEndpoint(ErrorUri);
			}
		}

		private ServiceBus CreateServiceBus()
		{
			var endpointFactory = ObjectBuilder.GetInstance<IEndpointFactory>();

			var endpoint = endpointFactory.GetEndpoint(_receiveFromUri);

			var subscriptionCache = ObjectBuilder.GetInstance<ISubscriptionCache>() ?? new LocalSubscriptionCache();
			var typeInfoCache = ObjectBuilder.GetInstance<ITypeInfoCache>() ?? new TypeInfoCache();

			return new ServiceBus(endpoint, ObjectBuilder, subscriptionCache, endpointFactory, typeInfoCache);
		}

		private void ConfigureThreadLimits(ServiceBus bus)
		{
			if (ConcurrentConsumerLimit > 0)
				bus.MaximumConsumerThreads = ConcurrentConsumerLimit;

			if (ConcurrentReceiverLimit > 0)
				bus.ConcurrentReceiveThreads = ConcurrentReceiverLimit;

			bus.ReceiveTimeout = ReceiveTimeout;
		}

		private void ConfigureBusServices(ServiceBus bus)
		{
			foreach (var serviceConfigurator in _services)
			{
				serviceConfigurator(bus, bus.SubscriptionCache, ObjectBuilder, bus.AddService);
			}
		}

		public static IServiceBus New(Action<IServiceBusConfigurator> action)
		{
			var configurator = new ServiceBusConfigurator();

			action(configurator);

			return configurator.Create();
		}

		public static void Defaults(Action<IServiceBusConfiguratorDefaults> action)
		{
			action(_defaults);
		}
	}
}