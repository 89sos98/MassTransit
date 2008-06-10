namespace MassTransit.WindsorIntegration
{
	using System;
	using System.Collections;
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using MassTransit.ServiceBus.Exceptions;
	using MassTransit.ServiceBus.HealthMonitoring;
	using MassTransit.ServiceBus.Internal;
	using MassTransit.ServiceBus.Subscriptions;
	using ServiceBus;

	public class MassTransitFacility :
		AbstractFacility
	{
		protected override void Init()
		{
			LoadTransports();
			RegisterComponents();

			LoadServiceBuses();
		}

		private void LoadTransports()
		{
			IConfiguration transportConfiguration = FacilityConfig.Children["transports"];
			if (transportConfiguration == null)
				throw new ConventionException("At least one transport must be defined in the facility configuration.");

			foreach (IConfiguration transport in transportConfiguration.Children)
			{
				Type t = Type.GetType(transport.Value, true, true);

				Kernel.AddComponent("transport." + t.Name, typeof (IEndpoint), t);
			}
		}

		private void RegisterComponents()
		{
			Kernel.AddComponentInstance("kernel", typeof (IKernel), Kernel);

			Kernel.Register(
				Component.For<ISubscriptionCache>()
					.ImplementedBy<LocalSubscriptionCache>()
					.LifeStyle.Transient,
				Component.For<IObjectBuilder>()
					.ImplementedBy<WindsorObjectBuilder>()
					.LifeStyle.Singleton,
				Component.For<IEndpointResolver>()
					.ImplementedBy<EndpointResolver>()
					.Named("endpoint.factory")
					.LifeStyle.Singleton,
				Component.For<IEndpoint>()
					.AddAttributeDescriptor("factoryId", "endpoint.factory")
					.AddAttributeDescriptor("factoryCreate", "Resolve")
				);
		}

		private void LoadServiceBuses()
		{
			foreach (IConfiguration child in FacilityConfig.Children)
			{
				if (child.Name.Equals("bus"))
				{
					string id = child.Attributes["id"];
					string endpointUri = child.Attributes["endpoint"];

					IEndpoint endpoint = ResolveEndpoint<IEndpoint>(endpointUri);

					ISubscriptionCache cache = ResolveSubscriptionCache(child);

					IServiceBus bus = new ServiceBus(endpoint,
					                                 Kernel.Resolve<IObjectBuilder>(),
					                                 cache,
					                                 Kernel.Resolve<IEndpointResolver>());

					Kernel.AddComponentInstance(id, typeof (IServiceBus), bus);

					ResolveSubscriptionClient(child, bus, id, cache);
					ResolveManagementClient(child, bus, id);
				}
			}
		}

		private void ResolveManagementClient(IConfiguration child, IServiceBus bus, string id)
		{
			IConfiguration managementClientConfig = child.Children["managementService"];
			if (managementClientConfig != null)
			{
				string heartbeatInterval = managementClientConfig.Attributes["heartbeatInterval"];

				int interval = string.IsNullOrEmpty(heartbeatInterval) ? 3 : int.Parse(heartbeatInterval);

				HealthClient sc = new HealthClient(bus, interval);

				Kernel.AddComponentInstance(id + ".managementClient", sc);
				sc.Start();
			}
		}

		private void ResolveSubscriptionClient(IConfiguration child, IServiceBus bus, string id, ISubscriptionCache cache)
		{
			IConfiguration subscriptionClientConfig = child.Children["subscriptionService"];
			if (subscriptionClientConfig != null)
			{
				string subscriptionServiceEndpointUri = subscriptionClientConfig.Attributes["endpoint"];

				IEndpoint subscriptionServiceEndpoint = ResolveEndpoint<IEndpoint>(subscriptionServiceEndpointUri);

				SubscriptionClient sc = new SubscriptionClient(bus, cache, subscriptionServiceEndpoint);

				Kernel.AddComponentInstance(id + ".subscriptionClient", sc);
				sc.Start();
			}
		}

		private ISubscriptionCache ResolveSubscriptionCache(IConfiguration configuration)
		{
			IConfiguration cacheConfig = configuration.Children["subscriptionCache"];
			if (cacheConfig == null)
				return Kernel.Resolve<ISubscriptionCache>();

			// naming the cache makes it available to others
			string name = cacheConfig.Attributes["name"];

			string mode = cacheConfig.Attributes["mode"];
			switch (mode)
			{
				case "local":
					if (string.IsNullOrEmpty(name))
						return Kernel.Resolve<ISubscriptionCache>();
					else
					{
						ISubscriptionCache cache = Kernel.Resolve<ISubscriptionCache>(name);
						if (cache == null)
						{
							cache = Kernel.Resolve<ISubscriptionCache>();
							Kernel.AddComponentInstance(name, cache);
						}

						return cache;
					}

				case "distributed":
					throw new NotImplementedException("Distributed cache mode not yet implemented");

				default:
					throw new ConventionException(mode + " is not a valid subscriptionCache mode");
			}
		}

		private T ResolveEndpoint<T>(string uri)
		{
			IDictionary arguments = new Hashtable();
			arguments.Add("uri", new Uri(uri));

			return Kernel.Resolve<T>(arguments);
		}

		private static ComponentRegistration<T> StartableComponent<T>()
		{
			return Component.For<T>()
				.AddAttributeDescriptor("startable", "true")
				.AddAttributeDescriptor("startMethod", "Start")
				.AddAttributeDescriptor("stopMethod", "Stop")
				.LifeStyle.Transient;
		}
	}
}