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
namespace DeferredMessageServiceHost
{
    using MassTransit.Host.LifeCycles;
    using MassTransit.ServiceBus;
    using MassTransit.Services;

    public class DeferredMessageLifeCycle :
        HostedLifeCycle
    {
        public DeferredMessageLifeCycle(string xmlFile) : base(xmlFile)
        {
        }

        public override void Start()
        {
            Container.AddComponent<IHostedService, MessageDeferralService>();
            Container.AddComponent<IDeferredMessageRepository, InMemoryDeferredMessageRepository>();
        }

        public override void Stop()
        {
            
        }
    }
}