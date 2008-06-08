namespace MassTransit.ServiceBus.Tests
{
    using System;
    using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture]
	public abstract class Specification
	{
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();

			Before_each();
		}

		[TearDown]
		public void Teardown()
		{
			After_each();

			_mocks = null;
		}

		private MockRepository _mocks;

		protected virtual void After_each()
		{
		}

		protected virtual void Before_each()
		{
		}

		protected T DynamicMock<T>()
		{
			return _mocks.DynamicMock<T>();
		}
        protected T StrictMock<T>()
        {
            return _mocks.CreateMock<T>();
        }

		protected T Stub<T>()
		{
			return _mocks.Stub<T>();
		}

		protected void ReplayAll()
		{
			_mocks.ReplayAll();
		}

		protected void Verify(object o)
		{
			_mocks.Verify(o);
		}

		protected void VerifyAll()
		{
			_mocks.VerifyAll();
		}

        protected IDisposable Record()
        {
            return _mocks.Record();
        }
        protected IDisposable Playback()
        {
            return _mocks.Playback();
        }
	}
}