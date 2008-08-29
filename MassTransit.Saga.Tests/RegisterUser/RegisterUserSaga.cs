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
namespace MassTransit.Saga.Tests.RegisterUser
{
    using System;
    using MassTransit.Saga.Messages;
    using Messages;
    using ServiceBus;

    /// <summary>
    /// 
    /// So here is the deal
    /// The saga is a class that contains the state and behavior of the saga
    /// for messages that are saga messages. If the message is not a saga message
    /// it doesn't need any details about the saga in order to proceed.
    /// 
    /// This will allow services that are not saga-specific to participate in a saga
    /// 
    /// So something like an e-mail sender would send an e-mail, publish the mail sent message
    /// and that would trigger the saga to continue according to the result.
    /// 
    /// By doing so, the saga can then return to power after being away for a while.
    /// 
    /// </summary>
    public class RegisterUserSaga :
        InitiatedBy<RegisterUser>,
        Orchestrates<UserVerificationEmailSent>,
        Orchestrates<UserValidated>,
        ISaga<RegisterUserSaga>
    {
        private string _displayName;
        private string _email;
        private string _password;
        private string _username;

        public RegisterUserSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }


        public Action<RegisterUserSaga> Save { get; set; }

        // The bus that received the message
        public IServiceBus Bus { get; set; }

        // the object builder for the class to be able to get other things
        public IObjectBuilder Builder { get; set; }

        public Guid CorrelationId { get; private set; }

        public void Dispose()
        {
        }

        public void Consume(UserValidated message)
        {
            // at this point, the user has clicked the link in the validation e-mail
            // and we can commit the user record to the database as a verified user

            Bus.Publish(new UserRegistrationComplete(CorrelationId));

            Complete();
        }

        public void Consume(UserVerificationEmailSent message)
        {
            // once the verification e-mail has been sent, we allow 24 hours to pass before we 
            // remove this transaction from the registration queue

            if (_email != message.Email)
                throw new ArgumentException("The email address was not properly loaded.");

            Bus.Publish(new UserRegistrationPending(CorrelationId));
            Bus.Publish(new UpdateWorkflowTimeout(CorrelationId, TimeSpan.FromHours(24)));
        }

        public void Consume(RegisterUser message)
        {
            CorrelationId = message.CorrelationId;
            _displayName = message.DisplayName;
            _username = message.Username;
            _password = message.Password;
            _email = message.Email;

            Save(this);

            // _sagaRepository.Save(this);

            Bus.Publish(new SendUserVerificationEmail(CorrelationId, _email));
        }

        private void Complete()
        {
            Bus.Publish(new CompleteWorkflow(CorrelationId));
        }
    }
}