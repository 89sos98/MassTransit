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
namespace MassTransit.ServiceBus.Timeout
{
	using System;
	using System.Collections.Generic;
	using Util;

	public interface ITimeoutStorage :
		IEnumerable<Guid>
	{
		void Schedule(Guid id, DateTime timeoutAt);
		void Remove(Guid id);

		IList<Tuple<Guid, DateTime>> List();

		void Start();
		void Stop();

		event Action<Guid> TimeoutAdded;
		event Action<Guid> TimeoutUpdated;
		event Action<Guid> TimeoutRemoved;
	}
}