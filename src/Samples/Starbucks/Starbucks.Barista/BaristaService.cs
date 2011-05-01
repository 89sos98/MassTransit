// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Starbucks.Barista
{
	using MassTransit;
	using MassTransit.Saga;

	public class BaristaService
	{
		private readonly IServiceBus _bus;
		private ISagaRepository<DrinkPreparationSaga> _sagaRepository;
		private UnsubscribeAction _unsubscribeAction;

		public BaristaService(IServiceBus bus, ISagaRepository<DrinkPreparationSaga> sagaRepository)
		{
			_bus = bus;
			_sagaRepository = sagaRepository;
		}

		public void Start()
		{
			_unsubscribeAction = _bus.SubscribeSaga(_sagaRepository);
		}

		public void Stop()
		{
			_unsubscribeAction();
			_bus.Dispose();
		}
	}
}