namespace MassTransit.ServiceBus.Tests.Subscriptions
{
	using System;
	using Messages;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;

	[TestFixture]
	public class When_using_the_subscription_service : 
		SubscriptionManagerContext
	{
		private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

		[Test]
		public void It_should_startup_properly()
		{
			Assert.That(SubscriptionCache.List(typeof(PingMessage).FullName).Count, Is.EqualTo(0));
		}

		[Test]
		public void A_subscription_should_end_up_on_the_service()
		{
			MonitorSubscriptionCache<PingMessage> monitor = new MonitorSubscriptionCache<PingMessage>(SubscriptionCache);

			LocalBus.AddComponent<TestMessageConsumer<PingMessage>>();

			monitor.ShouldHaveBeenAdded(_timeout);
		}

		[Test]
		public void A_subscription_should_be_removed_from_the_service()
		{
			MonitorSubscriptionCache<PingMessage> monitor = new MonitorSubscriptionCache<PingMessage>(SubscriptionCache);

			TestMessageConsumer<PingMessage> consumer = new TestMessageConsumer<PingMessage>();
			LocalBus.Subscribe(consumer);

			monitor.ShouldHaveBeenAdded(_timeout);

			LocalBus.Unsubscribe(consumer);

			monitor.ShouldHaveBeenRemoved(_timeout);
		}
	}
}