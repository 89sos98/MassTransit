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
namespace SubscriptionServiceHost
{
    using System.IO;
    using log4net;
    using log4net.Config;
    using MassTransit.Configuration;
    using MassTransit.Services.Subscriptions.Server;
    using MassTransit.WindsorIntegration;
    using Microsoft.Practices.ServiceLocation;
    using Topshelf;
    using Topshelf.Configuration;

    internal class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("subscriptionService.log4net.xml"));
            _log.Info("SubMgr Loading");

            IRunConfiguration cfg = RunnerConfigurator.New(c =>
                {
                    c.SetServiceName("MT-SUBSCRIPTIONS");
                    c.SetDisplayName("MassTransit Subscription Service");
                    c.SetDescription("Service to maintain message subscriptions");

                    c.RunAsFromInteractive();

                    c.DependencyOnMsmq();

                    c.BeforeStart(a =>
                        {
                            
                        });

                    c.ConfigureService<SubscriptionService>(s =>
                        {
                            s.CreateServiceLocator(()=>
                                {
                                    var container = new DefaultMassTransitContainer();

                                    container.AddComponent<ISubscriptionRepository, InMemorySubscriptionRepository>();
                                    container.AddComponent<SubscriptionService>();

                                    return container.ObjectBuilder;
                                });
                            s.WhenStarted(tc =>
                            {
                                var bus = ServiceBusConfigurator.New(sbc =>
                                    {
                                        sbc.ReceiveFrom("msmq://localhost/mt_subscriptions");
                                    });
                                tc.Start(bus);
                            });
                            s.WhenStopped(tc => tc.Stop());
                            s.WithName("Subscription service");
                        });
                });
            Runner.Host(cfg, args);
        }
    }
}