// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.Msmq.Tests
{
    using System.Transactions;
    using Magnum.Common.DateTimeExtensions;
    using Messages;
    using NUnit.Framework;

    [TestFixture]
    public class When_receiving_a_message
    {
        private readonly string uri = "msmq://localhost/mt_client_tx";
        private MsmqEndpoint ep;
        private DeleteMessage msg;

        private void Put_a_test_message_on_the_queue()
        {
            using (TransactionScope tr = new TransactionScope())
            {
                ep.Send(msg);
                tr.Complete();
            }
        }

        [SetUp]
        public void SetUp()
        {
            ep = new MsmqEndpoint(uri);
            QueueTestContext.ValidateAndPurgeQueue(ep.QueuePath, true);

            msg = new DeleteMessage();

            Put_a_test_message_on_the_queue();
        }

        [Test]
        public void From_A_Transactional_Queue_With_a_transaction()
        {
            using (TransactionScope tr = new TransactionScope())
            {
                ep.Receive(5.Hours());
                tr.Complete();
            }
        }

        [Test]
        public void From_A_Transactional_Queue_Without_a_transaction_should_auto_enlist()
        {
            object o = ep.Receive(5.Milliseconds());
            Assert.IsNotNull(o);
        }
    }
}