namespace MassTransit.ServiceBus.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;
    using Rhino.Mocks;

    [TestFixture]
    public class When_a_new_batch_is_received
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _bus = _mocks.CreateMock<IServiceBus>();
        }

        #endregion

        private MockRepository _mocks;
        private IServiceBus _bus;
        private BatchTransferController<BatchMessage, Guid> _controller;

        public void HandleBatch(BatchTransferController<BatchMessage, Guid> controller)
        {
            Guid batchId = controller.BatchId;

            foreach (BatchTransferMessage<Guid> message in controller)
            {
            }
        }

        [Test]
        public void Notify_the_subscriber_with_a_batch_message()
        {
            _controller = new BatchTransferController<BatchMessage, Guid>(HandleBatch);

            _bus.Subscribe<BatchMessage>(_controller.HandleMessage);
        }
    }

    public delegate void BatchControllerHandler<T, K>(BatchTransferController<T, K> controller) where T : BatchTransferMessage<K>;

    [Serializable]
    public class BatchMessage :
        BatchTransferMessage<Guid>
    {
    }

    public class BatchTransferController<T, K> :
        IEnumerable<T>,
        IMessage where T : BatchTransferMessage<K>
    {
        private readonly ManualResetEvent _complete = new ManualResetEvent(false);
        private readonly BatchControllerHandler<T, K> _handler;
        private readonly Semaphore _messageReady = new Semaphore(0, 0);
        private readonly Queue<T> _messages = new Queue<T>();

        private K _batchId;

        public BatchTransferController(BatchControllerHandler<T, K> handler)
        {
            _handler = handler;
        }

        private BatchTransferController(BatchControllerHandler<T, K> handler, IMessageContext<T> context)
        {
            _handler = handler;
            _batchId = context.Message.BatchId;

            context.Bus.Subscribe<T>(HandleMessage, 
                delegate(T message)
                    {
                        if (message.BatchId.Equals(_batchId))
                            return true;

                        return false;
                    });

            _messages.Enqueue(context.Message);
            _messageReady.Release();
        }

        public K BatchId
        {
            get { return _batchId; }
            set { _batchId = value; }
        }

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            WaitHandle[] handles = new WaitHandle[] { _messageReady, _complete };
            int waitResult;
            while ((waitResult = WaitHandle.WaitAny(handles, TimeSpan.FromMinutes(10), true)) != 1)
            {
                T message = _messages.Dequeue();

                yield return message;
            }

            if (waitResult == WaitHandle.WaitTimeout)
                throw new ApplicationException("Timeout waiting for batch to complete");
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        #endregion

        public void HandleMessage(IMessageContext<T> context)
        {
            K batchId = context.Message.BatchId;

            if (batchId.Equals(_batchId))
            {
                // this is a message for our batch, so add it to the queue and dispatch the enumerable handler
                _messages.Enqueue(context.Message);
                _messageReady.Release();
            }
            else
            {
                BatchTransferController<T,K> newController = new BatchTransferController<T, K>(_handler, context);

                // this is a new batch, we need to create a new controller and subscribe with the appropriate predicate to handle it
            }
        }
    }

    [Serializable]
    public abstract class BatchTransferMessage<K> :
        IMessage
    {
        private K _batchId;
        private long _batchLength;

        /// <summary>
        /// Identifies the batch containing this message
        /// </summary>
        public K BatchId
        {
            get { return _batchId; }
            set { _batchId = value; }
        }

        /// <summary>
        /// The number of messages in the batch
        /// </summary>
        public long BatchLength
        {
            get { return _batchLength; }
            set { _batchLength = value; }
        }
    }
}