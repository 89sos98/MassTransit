namespace MassTransit.ServiceBus.Tests
{
	using System;
	using Internal;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;

	[TestFixture]
	public class When_a_correlated_message_is_received
	{
		[Test]
		public void A_type_should_be_registered()
		{
			MessageDispatcher messageDispatcher = new MessageDispatcher();

			CorrelatedController controller = new CorrelatedController(messageDispatcher);

			controller.DoWork();

			Assert.That(controller.ResponseReceived, Is.True);
		}
	}

	internal class CorrelatedController :
		Consumes<ResponseMessage>.For<Guid>
	{
		private readonly MessageDispatcher _MessageDispatcher;
		private RequestMessage _request;

		private bool _responseReceived = false;

		public CorrelatedController(MessageDispatcher messageDispatcher)
		{
			_MessageDispatcher = messageDispatcher;
		}

		public bool ResponseReceived
		{
			get { return _responseReceived; }
		}

		public Guid CorrelationId
		{
			get { return _request.CorrelationId; }
		}

		public void Consume(ResponseMessage message)
		{
			_responseReceived = true;
		}

		public void DoWork()
		{
			_request = new RequestMessage();

			_MessageDispatcher.Subscribe(this);

			ResponseMessage response = new ResponseMessage(_request.CorrelationId);

			_MessageDispatcher.Consume(response);
		}
	}

	public class RequestMessage : CorrelatedBy<Guid>
	{
		private readonly Guid _correlationId = Guid.NewGuid();

		public Guid CorrelationId
		{
			get { return _correlationId; }
		}
	}

	public class ResponseMessage : CorrelatedBy<Guid>
	{
		private readonly Guid _correlationId;

        //xml serializer
        public ResponseMessage()
        {
        }

	    public ResponseMessage(Guid correlationId)
		{
			_correlationId = correlationId;
		}

		public Guid CorrelationId
		{
			get { return _correlationId; }
		}
	}
}