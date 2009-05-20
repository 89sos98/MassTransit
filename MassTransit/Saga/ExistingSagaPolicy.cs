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
namespace MassTransit.Saga
{
	using System;
	using Exceptions;

	public class ExistingSagaPolicy<TSaga, TMessage> :
		ISagaPolicy<TSaga, TMessage>
		where TSaga : ISaga
	{
		public bool CreateSagaWhenMissing(TMessage message, out Guid sagaId)
		{
			sagaId = Guid.Empty;

			return false;
		}

		public void ForExistingSaga(TMessage message)
		{
			// we are happy
		}

		public void ForMissingSaga(TMessage message)
		{
			throw new SagaException("Saga not found: " + message, typeof(TSaga), typeof(TMessage));
		}

		public bool SagaShouldExist
		{
			get { return true; }
		}

		public void ForExistingSaga(object message)
		{
		}
	}
}