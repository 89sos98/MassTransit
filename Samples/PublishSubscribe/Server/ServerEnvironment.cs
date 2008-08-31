namespace Server
{
    using MassTransit.Host;

	public class ServerEnvironment :
		HostedEnvironment
	{

		public ServerEnvironment(string xmlFile) : base(xmlFile)
		{
		}

	    public override string ServiceName
		{
			get { return "SampleServerService"; }
		}

		public override string DispalyName
		{
			get { return "MassTransit Sample Server Service"; }
		}

		public override string Description
		{
			get { return "Acts as a server on the service bus"; }
		}

	    public override HostedLifeCycle LifeCycle
	    {
            get { return new ServerLifeCycle(this.XmlFile); }
	    }
	}
}