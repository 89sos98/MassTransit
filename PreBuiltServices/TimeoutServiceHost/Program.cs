﻿// Copyright 2007-2008 The Apache Software Foundation.
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
namespace TimeoutServiceHost
{
    using System.Configuration;
    using System.IO;
    using log4net;
    using log4net.Config;
    using MassTransit;
    using MassTransit.Configuration;
    using MassTransit.Saga;
    using MassTransit.Services.HealthMonitoring.Configuration;
    using MassTransit.Services.Subscriptions.Configuration;
    using MassTransit.Services.Timeout;
    using MassTransit.Transports.Msmq;
    using MassTransit.WindsorIntegration;
    using Topshelf;
    using Topshelf.Configuration;

    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("timeoutService.log4net.xml"));
            _log.Info("Timeout Service Loading");

            IRunConfiguration cfg = RunnerConfigurator.New(c =>
            {
                c.SetServiceName("MT-TIMEOUT");
                c.SetDescription("Think Egg Timer");
                c.SetDisplayName("MassTransit Timeout Service");

                c.RunAsFromInteractive();

                c.DependencyOnMsmq();

                c.ConfigureService<TimeoutService>(typeof(TimeoutService).Name, s =>
                {
                    s.CreateServiceLocator(() =>
                    {
                        var container = new DefaultMassTransitContainer();
                        container.RegisterInMemorySagaRepository();
                        IEndpointFactory ef = EndpointFactoryConfigurator.New(e => e.RegisterTransport<MsmqEndpoint>());
                        container.Kernel.AddComponentInstance("endpointFactory", typeof (IEndpointFactory), ef);

                    	container.AddComponent("sagaRepository", typeof (ISagaRepository<>), typeof (InMemorySagaRepository<>));
                        container.AddComponent<TimeoutService>();
                        IServiceBus bus = ServiceBusConfigurator.New(sbc =>
                        {
                            sbc.ReceiveFrom(ConfigurationManager.AppSettings["receiveFrom"]);
                            sbc.ConfigureService<SubscriptionClientConfigurator>(sc => sc.SetSubscriptionServiceEndpoint(ConfigurationManager.AppSettings["subscriptionServiceEndpoint"]));
                            sbc.ConfigureService<HealthClientConfigurator>(hc => hc.SetHeartbeatInterval(10));
                        });
                        container.Kernel.AddComponentInstance<IServiceBus>(bus);
                        return container.ObjectBuilder;
                    });
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });
            Runner.Host(cfg, args);
        }
    }
}