namespace MassTransit.DistributedSubscriptionCacheX.Tests
{
	using System;
	using System.Collections.Generic;
	using Enyim.Caching;
	using MassTransit.ServiceBus.Subscriptions;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;

	[TestFixture]
	public class Given_a_message_name_that_has_three_subscriptions
	{
		protected readonly string _correlationId = Guid.NewGuid().ToString();
		protected readonly string _name = "CustomMessageName";

		protected readonly string _urlA = "http://localhost/default.html";
		protected readonly string _urlB = "http://localhost/index.html";
		protected readonly string _urlC = "http://localhost/feature.html";
		protected DistributedSubscriptionCache _cache;
		protected Subscription _subscriptionA;
		protected Subscription _subscriptionB;
		protected Subscription _subscriptionC;

		[SetUp]
		public void Setup()
		{
			MemcachedClient client = new MemcachedClient();

			client.Remove("/mt/" + _name);

			_cache = new DistributedSubscriptionCache();

			_subscriptionA = new Subscription(_name, new Uri(_urlA));
			_cache.Add(_subscriptionA);

			_subscriptionB = new Subscription(_name, new Uri(_urlB));
			_cache.Add(_subscriptionB);

			_subscriptionC = new Subscription(_name, new Uri(_urlC));
			_cache.Add(_subscriptionC);

			IList<Subscription> subscriptions = _cache.List(_name);

			Assert.That(subscriptions.Count, Is.EqualTo(3));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(_urlA));
			Assert.That(subscriptions[1].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[1].EndpointUri.ToString(), Is.EqualTo(_urlB));
			Assert.That(subscriptions[2].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[2].EndpointUri.ToString(), Is.EqualTo(_urlC));

			Before_each();
		}

		protected virtual void Before_each()
		{
		}

		[TearDown]
		public void Teardown()
		{
			_cache.Dispose();
			_cache = null;
		}
	}

	[TestFixture]
	public class When_the_first_url_is_removed : 
		Given_a_message_name_that_has_three_subscriptions
	{
		[Test]
		public void The_last_two_subscriptions_should_still_exist()
		{
			IList<Subscription> subscriptions = _cache.List(_name);

			Assert.That(subscriptions.Count, Is.EqualTo(2));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(_urlB));
			Assert.That(subscriptions[1].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[1].EndpointUri.ToString(), Is.EqualTo(_urlC));
		}

		protected override void Before_each()
		{
			_cache.Remove(_subscriptionA);
		}
	}

	[TestFixture]
	public class When_the_second_url_is_removed : 
		Given_a_message_name_that_has_three_subscriptions
	{
		[Test]
		public void The_last_two_subscriptions_should_still_exist()
		{
			IList<Subscription> subscriptions = _cache.List(_name);

			Assert.That(subscriptions.Count, Is.EqualTo(2));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(_urlA));
			Assert.That(subscriptions[1].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[1].EndpointUri.ToString(), Is.EqualTo(_urlC));
		}

		protected override void Before_each()
		{
			_cache.Remove(_subscriptionB);
		}
	}

	[TestFixture]
	public class When_the_last_url_is_removed : 
		Given_a_message_name_that_has_three_subscriptions
	{
		[Test]
		public void The_last_two_subscriptions_should_still_exist()
		{
			IList<Subscription> subscriptions = _cache.List(_name);

			Assert.That(subscriptions.Count, Is.EqualTo(2));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(_urlA));
			Assert.That(subscriptions[1].MessageName, Is.EqualTo(_name));
			Assert.That(subscriptions[1].EndpointUri.ToString(), Is.EqualTo(_urlB));
		}

		protected override void Before_each()
		{
			_cache.Remove(_subscriptionC);
		}
	}
}