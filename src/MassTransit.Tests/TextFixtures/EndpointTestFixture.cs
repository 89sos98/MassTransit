// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Tests.TextFixtures
{
	using System;
	using EndpointConfigurators;
	using Exceptions;
	using Magnum.Extensions;
	using MassTransit.Saga;
	using MassTransit.Transports;
	using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture]
	public abstract class EndpointTestFixture<TTransportFactory>
		where TTransportFactory : ITransportFactory, new()
	{
		[SetUp]
		public void Setup()
		{
			_endpointFactoryConfigurator.Validate();

			ObjectBuilder = MockRepository.GenerateMock<IObjectBuilder>();

			EndpointFactory = _endpointFactoryConfigurator.CreateEndpointFactory();
			_endpointFactoryConfigurator = null;

			EndpointCache = new EndpointCache(EndpointFactory);

			ServiceBusFactory.ConfigureDefaultSettings(x =>
				{
					x.SetEndpointCache(EndpointCache);
					x.SetConcurrentConsumerLimit(4);
					x.SetReceiveTimeout(50.Milliseconds());
					x.SetObjectBuilder(ObjectBuilder);
					x.EnableAutoStart();
				});

			EstablishContext();
		}

		[TearDown]
		public void Teardown()
		{
			TeardownContext();

			EndpointCache.Dispose();
			EndpointCache = null;
		}

		EndpointFactoryConfiguratorImpl _endpointFactoryConfigurator;

		protected EndpointTestFixture()
		{
			var defaultSettings = new EndpointFactoryDefaultSettings();

			_endpointFactoryConfigurator = new EndpointFactoryConfiguratorImpl(defaultSettings);
			_endpointFactoryConfigurator.AddTransportFactory<TTransportFactory>();
		}

		protected void AddTransport<T>()
			where T : ITransportFactory, new()
		{
			_endpointFactoryConfigurator.AddTransportFactory<T>();
		}

		protected IEndpointFactory EndpointFactory { get; private set; }
		protected IEndpointCache EndpointCache { get; set; }
		protected IObjectBuilder ObjectBuilder { get; private set; }

		protected virtual void EstablishContext()
		{
		}

		protected virtual void TeardownContext()
		{
		}

		protected void ConfigureEndpointFactory(Action<EndpointFactoryConfigurator> configure)
		{
			if (_endpointFactoryConfigurator == null)
				throw new ConfigurationException("The endpoint factory configurator has already been executed.");

			configure(_endpointFactoryConfigurator);
		}

		public static InMemorySagaRepository<TSaga> SetupSagaRepository<TSaga>(IObjectBuilder builder)
			where TSaga : class, ISaga
		{
			var sagaRepository = new InMemorySagaRepository<TSaga>();

			builder.Stub(x => x.GetInstance<ISagaRepository<TSaga>>())
				.Return(sagaRepository);

			return sagaRepository;
		}
	}
}