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
namespace MassTransit.Tests.Pipeline
{
	using MassTransit.Pipeline;
	using MassTransit.Pipeline.Configuration;
	using MassTransit.Pipeline.Configuration.Subscribers;
	using Messages;
	using NUnit.Framework;
	using Rhino.Mocks;

	[TestFixture]
	public class When_building_a_pipeline
	{
		[SetUp]
		public void Setup()
		{
			_builder = MockRepository.GenerateMock<IObjectBuilder>();
			_pipeline = MessagePipelineConfigurator.CreateDefault(_builder);
		}

		private IObjectBuilder _builder;
		private MessagePipeline _pipeline;

		[Test]
		public void The_builder_should_stay_with_the_pipeline()
		{
			var interceptor = MockRepository.GenerateMock<IPipelineSubscriber>();

			_pipeline.Configure(x => { x.Register(interceptor); });
		}

		[Test]
		public void The_pipeline_should_be_happy()
		{
			IndiscriminantConsumer<PingMessage> consumer = new IndiscriminantConsumer<PingMessage>();

			_pipeline.Subscribe(consumer);

			_pipeline.Dispatch(new PingMessage());

			Assert.IsNotNull(consumer.Consumed);
		}
	}
}