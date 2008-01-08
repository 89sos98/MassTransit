using System;

namespace MassTransit.ServiceBus
{
    using System.Collections.Generic;
    using log4net;

    public class MessageContext<T> :
        EventArgs where T : IMessage
    {
        private readonly IEnvelope _envelope;
        private readonly T _message;
        private readonly IServiceBus _bus;
        private readonly ILog _log = LogManager.GetLogger(typeof (MessageContext<T>));

        public MessageContext(IServiceBus bus, IEnvelope envelope, T message)
        {
            _envelope = envelope;
            _bus = bus;
            _message = message;
        }

        public IEnvelope Envelope
        {
            get { return _envelope; }
        }

        public T Message
        {
            get { return _message; }
        }

        public IServiceBus Bus
        {
            get { return _bus; }
        }

        /// <summary>
        /// Builds an envelope with the correlation id set to the id of the incoming envelope
        /// </summary>
        /// <param name="messages">The messages to include with the reply</param>
        public void Reply(params IMessage[] messages)
        {
            IEndpoint replyEndpoint = Envelope.ReturnEndpoint;

            IEnvelope envelope = new Envelope(Bus.Endpoint, messages);
            envelope.CorrelationId = Envelope.Id;

            IMessageSender send = MessageSenderFactory.Create(replyEndpoint);

            send.Send(envelope);
        }

        /// <summary>
        /// Moves the specified messages to the back of the list of available 
        /// messages so they can be handled later. Could screw up message order.
        /// </summary>
        public void HandleMessagesLater(params IMessage[] messages)
        {
            Bus.Send(Bus.Endpoint, messages);
        }

        /// <summary>
        /// Marks the whole context as poison
        /// </summary>
        public void MarkPoison()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Envelope {0} Was Marked Poisonous", _envelope.Id);

            MessageSenderFactory.Create(Bus.PoisonEndpoint).Send(_envelope);
        }

        /// <summary>
        /// Marks a specific message as poison
        /// </summary>
        public void MarkPoison(IMessage msg)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("A Message (Index:{1}) in Envelope {0} Was Marked Poisonous", _envelope.Id, new List<IMessage>(Envelope.Messages).IndexOf(msg));

            IEnvelope env = (IEnvelope) Envelope.Clone(); //Should this be cloned?
            env.Messages = new IMessage[] {Message};
            
            MessageSenderFactory.Create(Bus.PoisonEndpoint).Send(env);
        }
    }
}