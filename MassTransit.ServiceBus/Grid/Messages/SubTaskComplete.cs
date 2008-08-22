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
namespace MassTransit.Grid.Messages
{
	using System;

	[Serializable]
	public class SubTaskComplete<TResult>
		where TResult : class
	{
		private readonly int _activeTaskCount;
		private readonly Uri _address;
		private readonly TResult _result;
		private readonly int _subTaskId;
		private readonly Guid _taskId;
		private readonly int _taskLimit;

		public SubTaskComplete(Uri address, int taskLimit, int activeTaskCount, Guid taskId, int subTaskId, TResult result)
		{
			_address = address;
			_activeTaskCount = activeTaskCount;
			_taskLimit = taskLimit;
			_taskId = taskId;
			_subTaskId = subTaskId;
			_result = result;
		}

		public Uri Address
		{
			get { return _address; }
		}

		public int TaskLimit
		{
			get { return _taskLimit; }
		}

		public int ActiveTaskCount
		{
			get { return _activeTaskCount; }
		}

		public TResult Result
		{
			get { return _result; }
		}

		public Guid TaskId
		{
			get { return _taskId; }
		}

		public int SubTaskId
		{
			get { return _subTaskId; }
		}
	}
}