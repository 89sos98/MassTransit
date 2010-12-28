// Copyright 2007-2010 The Apache Software Foundation.
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
namespace MassTransit.Transports.RabbitMq
{
    using System;
    using Magnum;
    using MassTransit.Configuration;

    public class RabbitMqEndpointConfigurator :
        EndpointConfiguratorBase
    {
        public static IEndpoint New(Action<IEndpointConfigurator> action)
        {
            var configurator = new RabbitMqEndpointConfigurator();

            action(configurator);

            return configurator.Create();
        }

        private IEndpoint Create()
        {
            Guard.AgainstNull(Uri, "No Uri was specified for the endpoint");
            Guard.AgainstNull(SerializerType, "No serializer type was specified for the endpoint");

            IEndpoint endpoint = RabbitMqEndpointFactory.New(new CreateEndpointSettings(Uri)
            {
                Serializer = GetSerializer(),
            });

            return endpoint;
        }
    }
}