namespace MassTransit.ServiceBus.Tests.TestConsumers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    public class TestConsumerBase<TMessage>
        where TMessage : class
    {
        private static readonly List<TMessage> _allMessages = new List<TMessage>();
        private static readonly Semaphore _allReceived = new Semaphore(0, 100);
        private readonly List<TMessage> _messages = new List<TMessage>();
        private readonly Semaphore _received = new Semaphore(0, 100);

        public virtual void Consume(TMessage message)
        {
			Interlocked.Increment(ref _receivedMessageCount);

            _messages.Add(message);
            _received.Release();

            _allMessages.Add(message);
            _allReceived.Release();
        }

        public void MessageHandler(TMessage message)
        {
            _messages.Add(message);
            _received.Release();
        }

        private bool ReceivedMessage(TMessage message, TimeSpan timeout)
        {
            while (_messages.Contains(message) == false)
            {
                if (_received.WaitOne(timeout, true) == false)
                    return false;
            }

            return true;
        }

    	private int _receivedMessageCount;
    	public int ReceivedMessageCount
    	{
    		get { return _receivedMessageCount; }
    		protected set { _receivedMessageCount = value; }
    	}

    	public void ShouldHaveReceivedMessage(TMessage message, TimeSpan timeout)
        {
            Assert.That(ReceivedMessage(message, timeout), Is.True, "Message should have been received");
        }

        public void ShouldNotHaveReceivedMessage(TMessage message, TimeSpan timeout)
        {
            Assert.That(ReceivedMessage(message, timeout), Is.False, "Message should not have been received");
        }

        private static bool AnyReceivedMessage(TMessage message, TimeSpan timeout)
        {
            while (_allMessages.Contains(message) == false)
            {
                if (_allReceived.WaitOne(timeout, true) == false)
                    return false;
            }

            return true;
        }

        public static void AnyShouldHaveReceivedMessage(TMessage message, TimeSpan timeout)
        {
            Assert.That(AnyReceivedMessage(message, timeout), Is.True, "Message should have been received");
        }
    }
}