using System;

namespace MassTransit.ServiceBus.Tests
{
	[Serializable]
	public class PingMessage : IMessage
	{
        public PingMessage() {}
	}
}