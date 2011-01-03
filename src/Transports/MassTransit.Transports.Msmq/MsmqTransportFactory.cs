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
namespace MassTransit.Transports.Msmq
{
	using System;
	using Exceptions;

	public class MsmqTransportFactory :
        ITransportFactory
	{
	    public string Scheme
	    {
            get { return "msmq"; }
	    }

	    public ITransport New(CreateTransportSettings settings)
		{
			try
			{
				if (settings.Address.IsLocal)
					return NewLocalTransport(settings);

				return NewRemoteTransport(settings);
			}
			catch (Exception ex)
			{
				throw new TransportException(settings.Address.Uri, "Failed to create MSMQ transport", ex);
			}
		}

		private static ITransport NewLocalTransport(CreateTransportSettings settings)
		{
			ValidateLocalTransport(settings);

			if (settings.Transactional)
				return new TransactionalMsmqTransport(settings.Address);

			return new NonTransactionalMsmqTransport(settings.Address);
		}

		private static ITransport NewRemoteTransport(CreateTransportSettings settings)
		{
			if (settings.Address.IsTransactional)
				return new TransactionalMsmqTransport(settings.Address);

			return new NonTransactionalMsmqTransport(settings.Address);
		}

		private static void ValidateLocalTransport(CreateTransportSettings settings)
		{

			MsmqEndpointManagement.Manage(settings.Address, q =>
				{
					if (!q.Exists)
					{
						if (!settings.CreateIfMissing)
							throw new TransportException(settings.Address.Uri,
								"The transport does not exist and automatic creation is not enabled");

						q.Create(settings.Transactional || settings.Address.IsTransactional);
					}

					if (settings.RequireTransactional)
					{
						if (!q.IsTransactional && (settings.Transactional || settings.Address.IsTransactional))
							throw new TransportException(settings.Address.Uri,
								"The transport is non-transactional but a transactional transport was requested");
					}
				});
		}

        public void PurgeExistingMessagesIfRequested(CreateEndpointSettings settings)
        {
            if (settings.Address.IsLocal && settings.PurgeExistingMessages)
            {
                MsmqEndpointManagement.Manage(settings.Address, x => x.Purge());
            }
        }
	}
}