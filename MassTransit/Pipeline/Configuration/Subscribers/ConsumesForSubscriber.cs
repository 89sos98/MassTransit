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
namespace MassTransit.Pipeline.Configuration.Subscribers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Exceptions;
    using Magnum.Threading;
    using Sinks;

    public class ConsumesForSubscriber :
        PipelineSubscriberBase
    {
        private static readonly Type _interfaceType = typeof (Consumes<>.For<>);

        private readonly ReaderWriterLockedDictionary<Type, Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction>> _instances;

        public ConsumesForSubscriber()
        {
            _instances = new ReaderWriterLockedDictionary<Type, Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction>>();
        }

        protected virtual UnsubscribeAction Connect<TMessage, TKey>(ISubscriberContext context, Consumes<TMessage>.For<TKey> consumer)
            where TMessage : class, CorrelatedBy<TKey>
        {
            var correlatedConfigurator = CorrelatedMessageRouterConfigurator.For(context.Pipeline);

            var router = correlatedConfigurator.FindOrCreate<TMessage, TKey>();

            UnsubscribeAction result = router.Connect(consumer.CorrelationId, new InstanceMessageSink<TMessage>(message => consumer.Consume));

            UnsubscribeAction remove = context.SubscribedTo<TMessage,TKey>(consumer.CorrelationId);

            return () => result() && remove();
        }

        public override IEnumerable<UnsubscribeAction> Subscribe<TComponent>(ISubscriberContext context)
        {
            yield break;
        }

        public override IEnumerable<UnsubscribeAction> Subscribe<TComponent>(ISubscriberContext context, TComponent instance)
        {
            Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction> invoker = GetInvokerForInstance<TComponent>();
            if (invoker == null)
                yield break;

            foreach (Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction> action in invoker.GetInvocationList())
            {
                yield return action(this, context, instance);
            }
        }

        private Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction> GetInvokerForInstance<TComponent>()
        {
            Type componentType = typeof (TComponent);

            return _instances.Retrieve(componentType, () =>
            {
                Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction> invoker = null;

                // since we don't have it, we're going to build it

                foreach (Type interfaceType in componentType.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    Type genericType = interfaceType.GetGenericTypeDefinition();

                    if (genericType != _interfaceType)
                        continue;

                    Type[] types = interfaceType.GetGenericArguments();

                    Type messageType = types[0];

                    Type keyType = types[1];

                    MethodInfo genericMethod = FindMethod(GetType(), "Connect", new[] {messageType, keyType}, new[] {typeof (ISubscriberContext), typeof (TComponent)});
                    if (genericMethod == null)
                        throw new PipelineException(string.Format("Unable to subscribe for type: {0} ({1})",
                                                                  typeof (TComponent).FullName, messageType.FullName));

                    var interceptorParameter = Expression.Parameter(typeof (ConsumesForSubscriber), "interceptor");
                    var contextParameter = Expression.Parameter(typeof (ISubscriberContext), "context");
                    var instanceParameter = Expression.Parameter(typeof (object), "instance");

                    var instanceCast = Expression.Convert(instanceParameter, typeof (TComponent));

                    var call = Expression.Call(interceptorParameter, genericMethod, contextParameter, instanceCast);

                    var connector = Expression.Lambda<Func<ConsumesForSubscriber, ISubscriberContext, object, UnsubscribeAction>>(call, new[] { interceptorParameter, contextParameter, instanceParameter }).Compile();

                    if (invoker == null)
                    {
                        invoker = (interceptor, context, obj) =>
                        {
                            if (context.HasMessageTypeBeenDefined(messageType))
                                return () => true;

                            context.MessageTypeWasDefined(messageType);

                            return connector(interceptor, context, obj);
                        };
                    }
                    else
                    {
                        invoker += (interceptor, context, obj) =>
                        {
                            if (context.HasMessageTypeBeenDefined(messageType))
                                return () => true;

                            context.MessageTypeWasDefined(messageType);

                            return connector(interceptor, context, obj);
                        };
                    }
                }

                return invoker;
            });
        }
    }
}