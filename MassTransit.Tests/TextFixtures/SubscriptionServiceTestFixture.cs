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
namespace MassTransit.Tests.TextFixtures
{
	using System.Collections.Generic;
	using System.Threading;
	using Configuration;
	using MassTransit.Saga;
	using MassTransit.Services.Subscriptions;
	using MassTransit.Services.Subscriptions.Client;
	using MassTransit.Services.Subscriptions.Configuration;
	using MassTransit.Services.Subscriptions.Messages;
	using MassTransit.Services.Subscriptions.Server;
	using MassTransit.Services.Subscriptions.Server.Messages;
	using MassTransit.Transports;
	using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture]
	public class SubscriptionServiceTestFixture :
		EndpointTestFixture<LoopbackEndpoint>
	{
		public const string SubscriptionServiceEndpointAddress = "loopback://localhost/mt_subscriptions";
		private ISagaRepository<SubscriptionClientSaga> _subscriptionClientSagaRepository;
		private ISagaRepository<SubscriptionSaga> _subscriptionSagaRepository;
		public SubscriptionService SubscriptionService { get; private set; }
		public IServiceBus LocalBus { get; private set; }
		public IControlBus LocalControlBus { get; private set; }
		public IServiceBus RemoteBus { get; private set; }
		public IControlBus RemoteControlBus { get; private set; }
		public IServiceBus SubscriptionBus { get; private set; }
		public ISubscriptionRepository SubscriptionRepository { get; private set; }

		protected override void EstablishContext()
		{
			base.EstablishContext();

			SubscriptionBus = ServiceBusConfigurator.New(x =>
				{
					x.ReceiveFrom(SubscriptionServiceEndpointAddress);
					x.SetConcurrentConsumerLimit(1);
				});

			SetupSubscriptionService(ObjectBuilder);

			LocalControlBus = ControlBusConfigurator.New(x =>
			{
				x.ReceiveFrom("loopback://localhost/mt_client_control");

				x.PurgeBeforeStarting();
			});

			RemoteControlBus = ControlBusConfigurator.New(x =>
			{
				x.ReceiveFrom("loopback://localhost/mt_server_control");

				x.PurgeBeforeStarting();
			});

			LocalBus = ServiceBusConfigurator.New(x =>
				{
					x.ConfigureService<SubscriptionClientConfigurator>(y =>
						{
							// setup endpoint
							y.SetSubscriptionServiceEndpoint(SubscriptionServiceEndpointAddress);
						});
					x.ReceiveFrom("loopback://localhost/mt_client");
					x.UseControlBus(LocalControlBus);
				});

			RemoteBus = ServiceBusConfigurator.New(x =>
				{
					x.ConfigureService<SubscriptionClientConfigurator>(y =>
						{
							// setup endpoint
							y.SetSubscriptionServiceEndpoint(SubscriptionServiceEndpointAddress);
						});
					x.ReceiveFrom("loopback://localhost/mt_server");
					x.UseControlBus(RemoteControlBus);
				});
		}

		private void SetupSubscriptionService(IObjectBuilder builder)
		{
			//SubscriptionRepository = new InMemorySubscriptionRepository();
			SubscriptionRepository = MockRepository.GenerateMock<ISubscriptionRepository>();
			SubscriptionRepository.Expect(x => x.List()).Return(new List<Subscription>());
			builder.Stub(x => x.GetInstance<ISubscriptionRepository>())
				.Return(SubscriptionRepository);

			_subscriptionClientSagaRepository = SetupSagaRepository<SubscriptionClientSaga>(builder);
			SetupInitiateSagaStateMachineSink<SubscriptionClientSaga, AddSubscriptionClient>(SubscriptionBus, _subscriptionClientSagaRepository, builder);
			SetupOrchestrateSagaStateMachineSink<SubscriptionClientSaga, RemoveSubscriptionClient>(SubscriptionBus, _subscriptionClientSagaRepository, builder);
			SetupObservesSagaStateMachineSink<SubscriptionClientSaga, SubscriptionClientAdded>(SubscriptionBus, _subscriptionClientSagaRepository, builder);

			_subscriptionSagaRepository = SetupSagaRepository<SubscriptionSaga>(builder);
			SetupInitiateSagaStateMachineSink<SubscriptionSaga, AddSubscription>(SubscriptionBus, _subscriptionSagaRepository, builder);
			SetupOrchestrateSagaStateMachineSink<SubscriptionSaga, RemoveSubscription>(SubscriptionBus, _subscriptionSagaRepository, builder);
			SetupObservesSagaStateMachineSink<SubscriptionSaga, SubscriptionClientRemoved>(SubscriptionBus, _subscriptionSagaRepository, builder);

			SubscriptionService = new SubscriptionService(SubscriptionBus, SubscriptionRepository, EndpointFactory, _subscriptionSagaRepository, _subscriptionClientSagaRepository);

            SubscriptionService.Start();

			builder.Stub(x => x.GetInstance<SubscriptionClient>())
				.Return(null)
				.WhenCalled(invocation => { invocation.ReturnValue = new SubscriptionClient(EndpointFactory); });
		}


		protected override void TeardownContext()
		{
			RemoteBus.Dispose();
			RemoteBus = null;

			RemoteControlBus.Dispose();
			RemoteControlBus = null;

			LocalBus.Dispose();
			LocalBus = null;

			LocalControlBus.Dispose();
			LocalControlBus = null;

			Thread.Sleep(500);

			SubscriptionService.Stop();
			SubscriptionService.Dispose();
			SubscriptionService = null;

			SubscriptionBus.Dispose();
			SubscriptionBus = null;

			base.TeardownContext();
		}
	}
}