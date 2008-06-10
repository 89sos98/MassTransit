namespace WebRequestReply.Core
{
	using MassTransit.ServiceBus;
	using MassTransit.WindsorIntegration;

	public class Container :
		DefaultMassTransitContainer
	{
		private static readonly Container _container;

		static Container()
		{
			_container = new Container();
		}

		private Container()
			: base("castle.xml")
		{
			Resolve<IServiceBus>().Subscribe<RequestMessage>(HandleRequestMessage);
		}

		public static Container Instance
		{
			get { return _container; }
		}

		private static void HandleRequestMessage(IMessageContext<RequestMessage> ctx)
		{
			ResponseMessage response = new ResponseMessage(ctx.Message.CorrelationId, "Request: " + ctx.Message.Text);

			Instance.Resolve<IServiceBus>().Publish(response);
		}
	}
}