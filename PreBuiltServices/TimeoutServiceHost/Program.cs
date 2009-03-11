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
    using System.IO;
    using log4net;
    using log4net.Config;
    using MassTransit;
    using MassTransit.Services.Timeout;
    using MassTransit.WindsorIntegration;
    using Microsoft.Practices.ServiceLocation;
    using Topshelf;
    using Topshelf.Configuration;

    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("timeoutService.log4net.xml"));
            _log.Info("Timeout Service Loading");

            var cfg = RunnerConfigurator.New(c =>
                                                 {
                                                     c.SetServiceName("MT-TIMEOUT");
                                                     c.SetDescription("Think Egg Timer");
                                                     c.SetDisplayName("MassTransit Timeout Service");

                                                     c.RunAsFromInteractive();

                                                     c.DependencyOnMsmq();

                                                     c.BeforeStart(a =>
                                                                       {
                                                                           var container = new DefaultMassTransitContainer("timeoutService.castle.xml");
                                                                           container.AddComponent<ITimeoutRepository,InMemoryTimeoutRepository>();
                                                                           container.AddComponent<IHostedService, TimeoutService>();

                                                                           var wob = new WindsorObjectBuilder(container.Kernel);
                                                                           ServiceLocator.SetLocatorProvider(() => wob);
                                                                       });

                                                     c.ConfigureService<TimeoutService>( s =>
                                                                                             {
                                                                                                 s.WhenStarted(tc => tc.Start());
                                                                                                 s.WhenStopped(tc => tc.Stop());
                                                                                                 s.WithName("Timeout service");
                                                                                             });
                                                 });
            Runner.Host(cfg, args);
        }
    }
}
