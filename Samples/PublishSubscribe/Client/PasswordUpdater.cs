namespace Client
{
    using System;
    using log4net;
    using MassTransit.ServiceBus;
    using SecurityMessages;

    public class PasswordUpdater :
        Consumes<PasswordUpdateComplete>.All
    {
		private static readonly ILog _log = LogManager.GetLogger(typeof(PasswordUpdater));

		public void Consume(PasswordUpdateComplete message)
        {
			_log.InfoFormat("Global password update complete: {0} ({1})", message.ErrorCode, message.CorrelationId);
        }
    }
}