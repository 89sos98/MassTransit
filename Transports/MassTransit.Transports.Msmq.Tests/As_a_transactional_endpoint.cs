namespace MassTransit.Transports.Msmq.Tests
{
    using System.Transactions;
    using MassTransit.Tests;
    using NUnit.Framework;

    //[TestFixture]
    public class As_a_transactional_endpoint
    {

        //[TestFixture]
        public class When_in_a_transaction
        {
            private MsmqEndpoint _ep;

            [SetUp]
            public void SetUp()
            {
                QueueTestContext.ValidateAndPurgeQueue(".\\private$\\mt_client_tx", true);
                _ep = new MsmqEndpoint("msmq://localhost/mt_client_tx");
            }

            [TearDown]
            public void TearDown()
            {
                _ep = null;
            }


            [Test]
            public void While_writing_it_should_perisist_on_success()
            {
                using (TransactionScope trx = new TransactionScope())
                {
                    _ep.Send(new DeleteMessage());
                    trx.Complete();
                }

                using(TransactionScope trx = new TransactionScope())
                {
                    QueueTestContext.VerifyMessageInQueue(_ep, new DeleteMessage());
                    trx.Complete();
                }
            }

            [Test]
            public void While_writing_it_should_perisist_on_failure()
            {
                using (TransactionScope trx = new TransactionScope())
                {
                    _ep.Send(new DeleteMessage());
                    //no complete
                }

                using (TransactionScope trx = new TransactionScope())
                {
                    QueueTestContext.VerifyMessageNotInQueue(_ep);
                    trx.Complete();
                }
            }
        }

        [TestFixture]
        public class When_outside_a_transaction
        {
            private MsmqEndpoint _ep;

            [SetUp]
            public void SetUp()
            {
                QueueTestContext.ValidateAndPurgeQueue(".\\private$\\mt_client_tx", true);
                _ep = new MsmqEndpoint("msmq://localhost/mt_client_tx");
            }

            [TearDown]
            public void TearDown()
            {
                _ep = null;
            }

            
            [Test]
            public void It_should_auto_enlist_a_transaction_and_persist()
            {
                _ep.Send(new DeleteMessage());


                using (TransactionScope trx = new TransactionScope())
                {
                    QueueTestContext.VerifyMessageInQueue(_ep, new DeleteMessage());
                    trx.Complete();
                }
            }

        }
        
    }
}