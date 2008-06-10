namespace WebRequestReply.UI.MonoRail
{
	using Castle.Core.Resource;
	using Castle.Facilities.FactorySupport;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Registration;
	using Castle.MonoRail.Framework;
	using Castle.MonoRail.WindsorExtension;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Controllers;
	using MassTransit.WindsorIntegration;

	public class WebAppContainer :
		WindsorContainer
	{
		public WebAppContainer()
			: base(new XmlInterpreter(new ConfigResource()))
		{
			RegisterFacilities();
			LoadMassTransit();
			RegisterComponents();
		}

		protected void RegisterFacilities()
		{
			AddFacility("rails", new MonoRailFacility());
			AddFacility("startable", new StartableFacility());
		}

		protected void RegisterComponents()
		{
			//new castle config!!
			//http://hammett.castleproject.org/?p=286
			Register(AllTypes.Of<SmartDispatcherController>()
			         	.FromAssembly(typeof (DemoController).Assembly));
		}

		protected void LoadMassTransit()
		{
			AddFacility("factory.support", new FactorySupportFacility());
			AddFacility("masstransit", new MassTransitFacility());
//			AddComponent("masstransit.bus", typeof(IServiceBus), typeof(ServiceBus));
//            AddComponent("masstransit.bus.listen", typeof (IEndpoint), typeof (MsmqEndpoint));
//
//            AddComponent("masstransit.subscription.endpoint", typeof (IEndpoint), typeof (MsmqEndpoint));
//            AddComponent("masstransit.subscription.client", typeof (IHostedService), typeof (SubscriptionClient));
//            AddComponent("masstransit.cache", typeof (ISubscriptionCache), typeof (LocalSubscriptionCache));
		}
	}
}