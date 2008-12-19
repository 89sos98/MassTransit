namespace MassTransit.Tests.Subscriptions
{
	using System;
	using MassTransit.Subscriptions;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;
	using Rhino.Mocks;

	[TestFixture]
	public class When_Working_With_Subscription_Entries
	{

		[SetUp]
		public virtual void Before_Each_Test_In_The_Fixture()
		{
			_serviceBusEndPoint = _mocks.StrictMock<IEndpoint>();

			SetupResult.For(_serviceBusEndPoint.Uri).Return(new Uri(_serviceBusQueueName));

			_mocks.ReplayAll();
		}

		protected MockRepository _mocks = new MockRepository();

		protected IServiceBus _serviceBus;
		protected IEndpoint _serviceBusEndPoint;
		protected string _serviceBusQueueName = @"msmq://localhost/test_servicebus";

		[Test]
		public void Comparing_Two_Entries_Should_Return_True()
		{
			SubscriptionCacheEntry left = new SubscriptionCacheEntry(new Subscription("A", _serviceBusEndPoint.Uri));
			SubscriptionCacheEntry right = new SubscriptionCacheEntry(new Subscription("A", _serviceBusEndPoint.Uri));

			Assert.That(left, Is.EqualTo(right));
		}
	}
}