﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.SubscriptionBuilders
{
	using System;
	using System.Collections.Generic;
	using SubscriptionConnectors;
	using Subscriptions;

	public class ConsumerSubscriptionBuilder<TConsumer> :
		SubscriptionBuilder
		where TConsumer : class
	{
		readonly Func<Action<Action<TConsumer>>> _consumerFactory;
		readonly Func<UnsubscribeAction, ISubscriptionReference> _referenceFactory;
		readonly IList<ConsumerSubscriptionConnector> _connectors;

		public ConsumerSubscriptionBuilder(Func<Action<Action<TConsumer>>> consumerFactory,
		                                   Func<UnsubscribeAction, ISubscriptionReference> referenceFactory)
		{
			_consumerFactory = consumerFactory;
			_referenceFactory = referenceFactory;

			_connectors = new List<ConsumerSubscriptionConnector>();
		}

		public IEnumerable<ConsumerSubscriptionConnector> Connectors
		{
			get { return _connectors; }
		}

		public ISubscriptionReference Subscribe(IServiceBus bus)
		{
			UnsubscribeAction unsubscribe = bus.Subscribe<TConsumer>();

			return _referenceFactory(unsubscribe);
		}
	}
}