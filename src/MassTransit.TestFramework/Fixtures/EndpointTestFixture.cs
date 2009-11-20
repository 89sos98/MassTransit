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
namespace MassTransit.TestFramework.Fixtures
{
	using System;
	using System.Collections.Generic;
	using Configuration;
	using Magnum.DateTimeExtensions;
	using NUnit.Framework;
	using Rhino.Mocks;
	using Serialization;

	[TestFixture]
	public abstract class EndpointTestFixture<TEndpoint>
		where TEndpoint : IEndpoint
	{
		[TestFixtureSetUp]
		public void EndpointTestFixtureSetup()
		{
			SetupObjectBuilder();

			SetupMessageSerializer();

			SetupEndpointFactory();

			SetupServiceBusDefaults();
		}

		[TestFixtureTearDown]
		public void EndpointTestFixtureTeardown()
		{
			TeardownBuses();

			EndpointFactory.Dispose();
			EndpointFactory = null;
		}

		protected EndpointTestFixture()
		{
			Buses = new List<IServiceBus>();
		}

		protected virtual void SetupObjectBuilder()
		{
			ObjectBuilder = MockRepository.GenerateMock<IObjectBuilder>();
		}

		protected virtual void SetupMessageSerializer()
		{
			var serializer = new XmlMessageSerializer();
			ObjectBuilder.Stub(x => x.GetInstance<XmlMessageSerializer>()).Return(serializer);
		}

		protected virtual void SetupEndpointFactory()
		{
			EndpointFactory = EndpointFactoryConfigurator.New(x =>
				{
					x.SetObjectBuilder(ObjectBuilder);
					x.RegisterTransport<TEndpoint>();
					x.SetDefaultSerializer<XmlMessageSerializer>();

					ConfigureEndpointFactory(x);
				});
			ObjectBuilder.Stub(x => x.GetInstance<IEndpointFactory>()).Return(EndpointFactory);
		}

		protected virtual void ConfigureEndpointFactory(IEndpointFactoryConfigurator x)
		{
		}

		protected virtual void SetupServiceBusDefaults()
		{
			ServiceBusConfigurator.Defaults(x =>
				{
					x.SetObjectBuilder(ObjectBuilder);
					x.SetReceiveTimeout(500.Milliseconds());
					x.SetConcurrentConsumerLimit(Environment.ProcessorCount*2);
				});
		}

		private void TeardownBuses()
		{
			Buses.Each(bus => { bus.Dispose(); });
			Buses.Clear();
		}

		protected IList<IServiceBus> Buses { get; private set; }

		protected IEndpointFactory EndpointFactory { get; private set; }

		protected IObjectBuilder ObjectBuilder { get; private set; }
	}
}