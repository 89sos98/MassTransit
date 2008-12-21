namespace MassTransit.Tests.Subscriptions
{
    using System;
    using MassTransit.Internal;
    using MassTransit.Subscriptions;
    using MassTransit.Subscriptions.Messages;
    using MassTransit.Subscriptions.ServerHandlers;
    using NUnit.Framework;
    using Rhino.Mocks;

    [TestFixture]
    public class As_a_RemoveSubscriptionHandler
        : Specification
    {
        private RemoveSubscriptionHandler handle;
        private ISubscriptionCache _mockCache;
        private ISubscriptionRepository _mockRepository;
        private FollowerRepository _mockFollower;
        private IEndpointFactory _mockEndpointResolver;


        private RemoveSubscription msgRem;
        private Uri uri = new Uri("queue:\\bob");

        protected override void Before_each()
        {
            _mockEndpointResolver = StrictMock<IEndpointFactory>();
            _mockCache = StrictMock<ISubscriptionCache>();
            _mockRepository = StrictMock<ISubscriptionRepository>();
            _mockFollower = new FollowerRepository(_mockEndpointResolver);
            handle = new RemoveSubscriptionHandler(_mockCache, _mockRepository, _mockFollower);

            msgRem = new RemoveSubscription("bob", uri);
        }

        [Test]
        public void remove_subscriptions_from_messages()
        {
            using (Record())
            {
                Expect.Call(delegate { _mockCache.Remove(null); }).IgnoreArguments();
                Expect.Call(delegate { _mockRepository.Remove(msgRem.Subscription); });
            }
            using (Playback())
            {
                handle.Consume(msgRem);
            }
        }

    }
}