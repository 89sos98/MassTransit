namespace MassTransit.LegacySupport.Tests.Proxy
{
    using Contexts;
    using NUnit.Framework;
    using Rhino.Mocks;

    public class WhenProxyStarts :
        WithProxyService
    {
        public override void BecauseOf()
        {
            Service.Start();
        }

        [Test]
        public void ShouldSubscribeItself()
        {
            MockBus.AssertWasCalled(b=>b.Subscribe(Service));
        }

        [Test]
        public void ShouldSubscribeToSaga()
        {
            MockBus.AssertWasCalled(b=>b.Subscribe<LegacySubscriptionClientSaga>());
        }

        [Test]
        public void ShouldCaptureTheNewEndpoint()
        {
            MockEndpointFactory.AssertWasCalled(e=>e.GetEndpoint("msmq://localhost/new_subservice"));
        }
    }
}