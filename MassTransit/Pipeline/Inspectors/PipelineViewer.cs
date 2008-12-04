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
namespace MassTransit.Pipeline.Inspectors
{
	using System;
	using System.Text;
	using Sinks;

	public class PipelineViewer :
		PipelineInspectorBase
	{
		private readonly StringBuilder _text = new StringBuilder();
		private int _depth;

		public string Text
		{
			get { return _text.ToString(); }
		}

		protected override void IncreaseDepth()
		{
			_depth++;
		}

		protected override void DecreaseDepth()
		{
			_depth--;
		}

		public bool Inspect(MessagePipeline element)
		{
			Append("Pipeline");

			return true;
		}

		public bool Inspect<TMessage>(MessageRouter<TMessage> element) where TMessage : class
		{
			Append(string.Format("Routed ({0})", typeof (TMessage).Name));

			return true;
		}

		public bool Inspect<TMessage>(InstanceMessageSink<TMessage> sink) where TMessage : class
		{
			Append(string.Format("Consumed by Instance ({0})", typeof (TMessage).Name));

			return true;
		}

		public bool Inspect<TMessage, TKey>(CorrelatedMessageRouter<TMessage, TKey> sink)
			where TMessage : class, CorrelatedBy<TKey>
		{
			Append(string.Format("Correlated by {1} ({0})", typeof (TMessage).Name, typeof (TKey).Name));

			return true;
		}

		public bool Inspect<TComponent, TMessage>(ComponentMessageSink<TComponent, TMessage> sink)
			where TMessage : class
			where TComponent : class, Consumes<TMessage>.All
		{
			Type componentType = typeof (TComponent);

			string componentName = componentType.IsGenericType ? componentType.GetGenericTypeDefinition().FullName : componentType.FullName;

			Append(string.Format("Consumed by Component {0} ({1})", componentName, typeof(TMessage).Name));

			return true;
		}

		public bool Inspect<TComponent, TMessage>(SelectedComponentMessageSink<TComponent, TMessage> sink)
			where TMessage : class
			where TComponent : class, Consumes<TMessage>.Selected
		{
			Append(string.Format("Conditionally Consumed by Component {0} ({1})", typeof (TComponent).FullName, typeof (TMessage).Name));

			return true;
		}

		public bool Inspect<TInput, TOutput>(MessageTranslator<TInput, TOutput> translator) where TInput : class where TOutput : class, TInput
		{
			Append(string.Format("Translated from {0} to {1}", typeof (TInput).FullName, typeof (TOutput).FullName));

			return true;
		}

		private void Pad()
		{
			_text.Append(new string('\t', _depth));
		}

		private void Append(string text)
		{
			Pad();

			_text.AppendFormat(text).AppendLine();
		}

		public static void Trace(MessagePipeline pipeline)
		{
			PipelineViewer viewer = new PipelineViewer();

			pipeline.Inspect(viewer);

			System.Diagnostics.Trace.WriteLine(viewer.Text);
		}
	}
}