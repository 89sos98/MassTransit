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
namespace MassTransit.WindsorIntegration
{
    using Castle.Facilities.FactorySupport;
    using Castle.Facilities.Startable;
    using Castle.Windsor;
    using Castle.Windsor.Configuration;

    public class DefaultMassTransitContainer :
        WindsorContainer
    {
        public DefaultMassTransitContainer()
        {
            Initialize();
        }

        public DefaultMassTransitContainer(string xmlFile)
            : base(xmlFile)
        {
            Initialize();
        }

        public DefaultMassTransitContainer(IConfigurationInterpreter configurationInterpreter)
            : base(configurationInterpreter)
        {
            Initialize();
        }

        public void Initialize()
        {
            AddFacility("startable", new StartableFacility());
            AddFacility("factory", new FactorySupportFacility());
            AddFacility("masstransit", new MassTransitFacility());
        }
    }
}