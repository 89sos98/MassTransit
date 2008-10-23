using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

namespace OpenAllNight.PubSub
{
    using Castle.Core;
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

            var container = new DefaultMassTransitContainer("pubsub.castle.xml");

            container.AddComponentLifeStyle("followerrepository", typeof (FollowerRepository), LifestyleType.Singleton);
            container.AddComponentLifeStyle("addsubscriptionhandler", typeof (AddSubscriptionHandler),
                                            LifestyleType.Transient);
            container.AddComponentLifeStyle("removesubscriptionhandler", typeof (RemoveSubscriptionHandler),
                                            LifestyleType.Transient);
            container.AddComponentLifeStyle("cacheupdaterequesthandler", typeof (CacheUpdateRequestHandler),
                                            LifestyleType.Transient);
            container.AddComponentLifeStyle("cancelupdaterequesthandler", typeof (CancelUpdatesHandler),
                                            LifestyleType.Transient);

            container.AddComponent<IHostedService, SubscriptionService>();

            container.AddComponent<ISubscriptionRepository, InMemorySubscriptionRepository>();

            var wob = new WindsorObjectBuilder(container.Kernel);
            ServiceLocator.SetLocatorProvider(() => wob);
            IInstallationConfiguration cfg = new SubscriptionManagerEnvironment(ServiceLocator.Current);

            Runner.Run(cfg, args);
        }
    }
}