// Copyright 2007-2011 The Apache Software Foundation.
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
namespace MassTransit.Transports
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Exceptions;
    using Internal;
    using Magnum;
    using Serialization;

    public class EndpointConfigurator :
        EndpointConfiguratorBase
    {
        static readonly EndpointDefaults _defaults = new EndpointDefaults();
        readonly ITransportFactory _factory;

        public EndpointConfigurator(ITransportFactory factory)
        {
            _factory = factory;
        }


        public IEndpoint New(Action<IEndpointConfigurator> action)
        {
            action(this);

            Guard.AgainstNull(Uri, "No Uri was specified for the endpoint");
            if (MessageSerializer == null)
                Guard.AgainstNull(SerializerType, "No serializer type was specified for the endpoint");

            var settings = new CreateEndpointSettings(Uri)
                               {
                                   Serializer = GetSerializer(),
                                   CreateIfMissing = _defaults.CreateMissingQueues,
                                   PurgeExistingMessages = _defaults.PurgeOnStartup,
                                   Transactional = _defaults.CreateTransactionalQueues,
                               };

            try
            {
                Guard.AgainstNull(settings.Address, "An address for the endpoint must be specified");
                Guard.AgainstNull(settings.ErrorAddress, "An error address for the endpoint must be specified");
                Guard.AgainstNull(settings.Serializer, "A message serializer for the endpoint must be specified");

                ITransport transport = _factory.New(settings.ToTransportSettings());

                _factory.PurgeExistingMessagesIfRequested(settings);

                var errorSettings = new CreateEndpointSettings(settings.ErrorAddress, settings);
                if (transport.Address.IsTransactional)
                    settings.Transactional = true;
                ITransport errorTransport = _factory.New(errorSettings.ToTransportSettings());

                var endpoint = new Endpoint(settings.Address, settings.Serializer, transport, errorTransport);

                return endpoint;
            }
            catch (Exception ex)
            {
                throw new EndpointException(settings.Address.Uri,
                                            "Failed to create '{0}' endpoint".FormatWith(_factory.Scheme), ex);
            }
        }


        public static void Defaults(Action<IEndpointDefaults> configureDefaults)
        {
            configureDefaults(_defaults);
        }
    }

    public class EndpointConfiguratorBase :
        IEndpointConfigurator
    {
        protected Type SerializerType { get; private set; }
        protected IMessageSerializer MessageSerializer { get; private set; }
        protected Uri Uri { get; private set; }

        public void SetSerializer<T>()
            where T : IMessageSerializer
        {
            SerializerType = typeof (T);
        }

        public void SetSerializer(IMessageSerializer serializer)
        {
            MessageSerializer = serializer;
        }

        public void SetSerializer(Type serializerType)
        {
            SerializerType = serializerType;
        }

        public void SetUri(Uri uri)
        {
            Uri = uri;
        }

        protected IMessageSerializer GetSerializer()
        {
            if (MessageSerializer != null) return MessageSerializer;

            NewExpression newExpression = Expression.New(SerializerType);
            Func<IMessageSerializer> maker = Expression.Lambda<Func<IMessageSerializer>>(newExpression).Compile();

            IMessageSerializer serializer = maker();

            if (serializer == null)
                throw new ConfigurationException("Unable to create message serializer " + SerializerType.FullName);

            return serializer;
        }
    }
}