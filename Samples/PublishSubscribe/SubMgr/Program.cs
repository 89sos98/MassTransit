namespace SubMgr
{
    using Castle.Core;
    using Castle.Windsor;
    using log4net;
    using MassTransit.Host;
    using MassTransit.ServiceBus;
    using MassTransit.ServiceBus.Subscriptions;
    using MassTransit.ServiceBus.Subscriptions.ServerHandlers;
    using MassTransit.WindsorIntegration;
    using Microsoft.Practices.ServiceLocation;

    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            _log.Info("SubMgr Loading");
            IWindsorContainer container = new DefaultMassTransitContainer("castle.xml");

            container.AddComponentLifeStyle<FollowerRepository>(LifestyleType.Singleton);


            container.AddComponent<AddSubscriptionHandler>();
            container.AddComponent<RemoveSubscriptionHandler>();
            container.AddComponent<CancelUpdatesHandler>();
            container.AddComponent<CacheUpdateRequestHandler>();
            container.AddComponent<IHostedService, SubscriptionService>();


            container.AddComponent<ISubscriptionRepository, InMemorySubscriptionRepository>();

            var wob = new WindsorObjectBuilder(container.Kernel);
            ServiceLocator.SetLocatorProvider(()=>wob);

            var credentials = Credentials.LocalSystem;
            var settings = WinServiceSettings.Custom(
                "SampleSubscriptionService",
                "MassTransit Sample Subscription Service",
                "Coordinates subscriptions between multiple systems",
                KnownServiceNames.Msmq);
            var lifecycle = new SubscriptionManagerLifeCycle(ServiceLocator.Current);

            Runner.Run(credentials, settings, lifecycle, args);
        }
    }
}