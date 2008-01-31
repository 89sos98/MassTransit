namespace MassTransit.ServiceBus.Tests.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using MassTransit.ServiceBus.Subscriptions;
    using MassTransit.ServiceBus.Subscriptions.Messages;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Rhino.Mocks.Constraints;

    [TestFixture]
    public class When_using_the_subscription_manager
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _cache = _mocks.CreateMock<ISubscriptionStorage>();
            _serviceBus = _mocks.CreateMock<IServiceBus>();
            _managerEndpoint = _mocks.CreateMock<IMessageQueueEndpoint>();
            _client = new SubscriptionManagerClient(_serviceBus, _cache, _managerEndpoint);
        }

        [TearDown]
        public void Teardown()
        {
            _client.Dispose();
        }

        #endregion

        private MockRepository _mocks;
        private SubscriptionManagerClient _client;
        private IServiceBus _serviceBus;
        private IMessageQueueEndpoint _managerEndpoint;
        private ISubscriptionStorage _cache;

        [Test]
        public void The_client_should_request_an_update_at_startup()
        {
            IServiceBusAsyncResult asyncResult = _mocks.CreateMock<IServiceBusAsyncResult>();

            using (_mocks.Record())
            {
                Expect.Call(delegate { _cache.SubscriptionChanged += null; }).IgnoreArguments();
                Expect.Call(delegate { _serviceBus.Subscribe<SubscriptionChange>(null); }).IgnoreArguments();
                Expect.Call(_serviceBus.Request<CacheUpdateRequest>(_managerEndpoint, (AsyncCallback) null, (object) null, null))
                    .IgnoreArguments()
                    .Constraints(Is.Equal(_managerEndpoint),
                        Is.Anything(),
                        Is.Anything(),
                        Is.Matching<CacheUpdateRequest[]>(
                            delegate(CacheUpdateRequest[] obj) { return true; }))
                    .Return(asyncResult);
            }

            using (_mocks.Playback())
            {
                _client.Start();
            }
        }

        [Test]
        public void The_client_should_update_the_cache_when_a_SubscriptionChange_message_is_received()
        {
            Subscription sub = new Subscription(new Uri("msmq://localhost/test"), "Ho.Pimp, Ho");

            SubscriptionChange change = new SubscriptionChange(sub, SubscriptionChangeType.Add);

            IMessageContext<SubscriptionChange> context = _mocks.CreateMock<IMessageContext<SubscriptionChange>>();

            using (_mocks.Record())
            {
                Expect.Call(context.Message).Return(change).Repeat.AtLeastOnce();

                _cache.Add(sub.MessageName, sub.Address);
            }

            using (_mocks.Playback())
            {
                _client.HandleSubscriptionChange(context);
            }
        }

        [Test]
        public void The_client_should_update_the_local_subscription_cache()
        {
            IServiceBusAsyncResult asyncResult = _mocks.CreateMock<IServiceBusAsyncResult>();

            List<Subscription> subscriptions = new List<Subscription>();
            subscriptions.Add(new Subscription(new Uri("msmq://localhost/test"), "Ho.Pimp, Ho"));

            CacheUpdateResponse cacheUpdateResponse = new CacheUpdateResponse(subscriptions);

            using (_mocks.Record())
            {
                Expect.Call(asyncResult.Messages).Return(new IMessage[] {cacheUpdateResponse}).Repeat.AtLeastOnce();

                _cache.Add(subscriptions[0].MessageName, subscriptions[0].Address);
            }

            using (_mocks.Playback())
            {
                _client.CacheUpdateResponse_Callback(asyncResult);
            }
        }

        [Test]
        public void When_a_local_service_subscribes_to_the_bus_notify_the_manager_of_the_change()
        {
            SubscriptionChangedEventArgs args = new SubscriptionChangedEventArgs(new SubscriptionChange("Ho.Pimp, Ho", new Uri("msmq://" + Environment.MachineName.ToLower() + "/test"), SubscriptionChangeType.Add));

            using (_mocks.Record())
            {
                _serviceBus.Send<SubscriptionChange>(_managerEndpoint, null);
                LastCall
                    .IgnoreArguments()
                    .Constraints(Is.Equal(_managerEndpoint),
                        Is.Matching<SubscriptionChange[]>(
                            delegate(SubscriptionChange[] obj)
                                {
                                    if (obj[0].Subscription.Address != args.Change.Subscription.Address)
                                        return false;

                                    if (obj[0].Subscription.MessageName != args.Change.Subscription.MessageName)
                                        return false;

                                     return true;
                                }));
            }

            using (_mocks.Playback())
            {
                _client.Cache_SubscriptionChanged(_cache, args);
            }
        }

    }
}