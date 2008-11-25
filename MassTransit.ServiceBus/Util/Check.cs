/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.

namespace MassTransit.Util
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using Exceptions;

    /// <summary>
    /// Check class for verifying the condition of items included in interface contracts
    /// </summary>
    public static class Check
    {
        private static bool _useExceptions = true;

        public static bool UseExceptions
        {
            get { return _useExceptions; }
            set { _useExceptions = value; }
        }

        public static ObjectCheck Parameter(object objectUnderCheck)
        {
            return new ObjectCheck(objectUnderCheck);
        }

        public static StringCheck Parameter(string stringUnderCheck)
        {
            return new StringCheck(stringUnderCheck);
        }

        public static void ThrowArgumentNullException(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException();
            else
                throw new ArgumentNullException("", message);
        }

        public static void Require(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PreconditionException(message);
            }
            else
            {
                Trace.Assert(assertion, "Precondition: " + message);
            }
        }

        public static void Ensure(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PostconditionException(message);
            }
            else
            {
                Trace.Assert(assertion, "Postcondition: " + message);
            }
        }

        public static void EnsureSerializable(object message)
        {
            Check.Parameter(message).IsNotNull();

            Type t = message.GetType();
            if (!t.IsSerializable)
            {
                throw new ConventionException("Messages must be marked with the 'Serializable' attribute!");
            }
        }

        #region Nested type: ObjectCheck

        public class ObjectCheck
        {
            private readonly object _objectUndercheck;
            private string _message;

            public ObjectCheck(object objectUnderCheck)
            {
                _objectUndercheck = objectUnderCheck;
            }

            public void IsNotNull()
            {
                if (_objectUndercheck == null)
                    ThrowArgumentNullException(_message);
            }

            public ObjectCheck WithMessage(string message)
            {
                _message = message;

                return this;
            }
        }

        #endregion

        #region Nested type: StringCheck

        public class StringCheck
        {
            private readonly string _stringUnderCheck;
            private string _message;

            public StringCheck(string stringUnderCheck)
            {
                _stringUnderCheck = stringUnderCheck;
            }

            public void IsNotNullOrEmpty()
            {
                if (string.IsNullOrEmpty(_stringUnderCheck))
                {
                    ThrowArgumentNullException(_message);
                }
            }

            public StringCheck WithMessage(string message)
            {
                _message = message;

                return this;
            }
        }

        #endregion
    }

    public class CheckException : Exception
    {
        protected CheckException()
        {
        }

        protected CheckException(string message) : base(message)
        {
        }

        protected CheckException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class PreconditionException : CheckException
    {
        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException()
        {
        }

        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Exception raised when a postcondition fails.
    /// </summary>
    public class PostconditionException : CheckException
    {
        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException()
        {
        }

        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}