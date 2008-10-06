namespace PostalService.Host
{
	using System;
	using MassTransit.Host.LifeCycles;
    using MassTransit.ServiceBus;

	public class PostalServiceLifeCycle :
        HostedLifeCycle
    {
        public PostalServiceLifeCycle(string xmlFile) : base(xmlFile)
        {
        }
     private IServiceBus _bus;

        public override void Start()
        {
			Container.AddComponent<SendEmailConsumer>();

            _bus = Container.Resolve<IServiceBus>("server");

			_bus.AddComponent<SendEmailConsumer>();

            Console.WriteLine("Service running...");
        }

        public override void Stop()
        {
            Console.WriteLine("Service exiting...");

            _bus.Dispose();
        }
    }
}