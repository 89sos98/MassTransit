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
namespace MassTransit.Tests.Timeouts
{
	using System;
	using System.Diagnostics;
	using Magnum;
	using Magnum.DateTimeExtensions;
	using MassTransit.Services.MessageDeferral;
	using MassTransit.Services.MessageDeferral.Messages;
	using MassTransit.Services.Timeout;
	using Messages;
	using NUnit.Framework;
	using Rhino.Mocks;
	using TestConsumers;
	using TextFixtures;

	[TestFixture]
	public class When_a_message_is_deferred :
		LoopbackLocalAndRemoteTestFixture
	{
		private ITimeoutRepository _timeoutRepository;
		private TimeoutService _timeoutService;
		private Guid _correlationId;
		private IDeferredMessageRepository _repository;
		private MessageDeferralService _deferService;

		protected override void EstablishContext()
		{
			base.EstablishContext();

			_correlationId = CombGuid.Generate();

			_timeoutRepository = new InMemoryTimeoutRepository();
			ObjectBuilder.Stub(x => x.GetInstance<ITimeoutRepository>()).Return(_timeoutRepository);


			ObjectBuilder.Stub(x => x.GetInstance<ScheduleTimeoutConsumer>()).Return(new ScheduleTimeoutConsumer(_timeoutRepository));
			ObjectBuilder.Stub(x => x.GetInstance<CancelTimeoutConsumer>()).Return(new CancelTimeoutConsumer(_timeoutRepository));

            _timeoutService = new TimeoutService(RemoteBus, _timeoutRepository);
            _timeoutService.Start();


			_repository = new InMemoryDeferredMessageRepository();
			ObjectBuilder.Stub(x => x.GetInstance<IDeferredMessageRepository>()).Return(_repository);
			ObjectBuilder.Stub(x => x.GetInstance<DeferMessageConsumer>()).Return(new DeferMessageConsumer(RemoteBus, _repository));
			ObjectBuilder.Stub(x => x.GetInstance<TimeoutExpiredConsumer>()).Return(new TimeoutExpiredConsumer(RemoteBus, _repository));

            _deferService = new MessageDeferralService(RemoteBus);
            _deferService.Start();
		}

		protected override void TeardownContext()
		{
			base.TeardownContext();

			_deferService.Stop();
			_deferService.Dispose();

			_timeoutService.Stop();
			_timeoutService.Dispose();
		}

		[Test]
		public void It_should_be_received_after_the_deferral_period_elapses()
		{
			TestMessageConsumer<PingMessage> consumer = new TestMessageConsumer<PingMessage>();
			LocalBus.Subscribe(consumer);

			Stopwatch watch = Stopwatch.StartNew();

			var message = new PingMessage();

			LocalBus.Publish(new DeferMessage(_correlationId, 1.Seconds(), message));

			consumer.ShouldHaveReceivedMessage(message, 3.Seconds());

			watch.Stop();

			Debug.WriteLine(string.Format("Timeout took {0}ms", watch.ElapsedMilliseconds));
		}
	}
}