namespace PostalService.Host
{
    using MassTransit.Host.Configurations;
    using MassTransit.Host.LifeCycles;

    public class PostalServiceConfiguration :
        LocalSystemConfiguration
    {
        private IApplicationLifeCycle _lifeCycle;

        public PostalServiceConfiguration(string xmlFile)
		{
		    _lifeCycle = new PostalServiceLifeCycle(xmlFile);
		}

	    public override string ServiceName
		{
			get { return "PostalService"; }
		}

		public override string DisplayName
		{
			get { return "Sample Email Service"; }
		}

		public override string Description
		{
			get { return "We goin' postal!"; }
		}

	    public override IApplicationLifeCycle LifeCycle
	    {
            get { return _lifeCycle; }
	    }
    }
}