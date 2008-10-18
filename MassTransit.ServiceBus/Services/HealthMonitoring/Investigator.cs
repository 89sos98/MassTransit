// Copyright 2007-2008 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.ServiceBus.Services.HealthMonitoring
{
    using System;
    using System.Timers;
    using Internal;
    using log4net;
    using Messages;

    public class Investigator : 
        Consumes<Suspect>.All,
        Consumes<Pong>.All
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (Investigator));
        private readonly IServiceBus _bus;
        private readonly Guid _correlationId;
        private Suspect _suspectMessage;
        private readonly Ping _pingMessage;
        private readonly IEndpointResolver _resolver;
        private readonly Timer _timer;
        private readonly double _timeout;
        private readonly IHealthCache _healthCache;

        public Investigator(IServiceBus bus, IEndpointResolver resolver, IHealthCache healthCache):this(bus, resolver, healthCache, (1000*60*3)+50)
        {
            _healthCache = healthCache;
        }

        public Investigator(IServiceBus bus, IEndpointResolver resolver, IHealthCache healthCache, double timeout)
        {
            _bus = bus;
            _healthCache = healthCache;
            _timeout = timeout;
            _resolver = resolver;
            _correlationId = Guid.NewGuid();
            _pingMessage = new Ping(this.CorrelationId);
            _timer = new Timer(_timeout);
            _timer.Elapsed += OnPingTimeOut;
        }


        //this starts things
        public void Consume(Suspect msg)
        {
            _suspectMessage = msg;

            IEndpoint ep = _resolver.Resolve(msg.EndpointUri);

            ep.Send(_pingMessage, new TimeSpan(0,3,0));
            _timer.Start();
        }


        public void Consume(Pong msg)
        {
            //if we get this we are ok. but its weird that the heartbeat is down
            _timer.Stop();
            _log.WarnFormat("The endpoint '{0}' is responsive, but not sending heartbeats.", msg.EndpointUri);
        }

        public void OnPingTimeOut(object  sender, ElapsedEventArgs args)
        {
            //I have a confirmed dead endpoint
            _bus.Publish(new DownEndpoint(_suspectMessage.EndpointUri));

            HealthInformation information = _healthCache.Get(_suspectMessage.EndpointUri);
            information.LastFaultDetectedAt = DateTime.Now;
            _healthCache.Update(information);
            
            _timer.Stop();
            _timer.Dispose();
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }


        public bool Enabled
        {
            get { return _timer.Enabled; }
        }
    }
}