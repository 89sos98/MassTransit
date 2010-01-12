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
namespace MassTransit.LegacySupport.Tests.OldSerializedMessages
{
    using System.Runtime.Serialization.Formatters.Binary;
    using NUnit.Framework;
    using System.Collections.Generic;
    using SerializationCustomization;
    using Subscriptions;

    [TestFixture]
    public class TestSerialization
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Old = new BinaryFormatter();
            NewReader = new BinaryFormatter();
            NewWriter = new BinaryFormatter();

            var readerSelector = new LegacySurrogateSelector();
            readerSelector.AddSurrogate(new WeakToStrongListSurrogate<List<Subscription>, Subscription>("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Collections.Generic.List`1[[MassTransit.ServiceBus.Subscriptions.Subscription, MassTransit.ServiceBus, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"));
            
            
            NewReader.SurrogateSelector = readerSelector;
            
            
            //smelly
            var maps = new TypeMappings();
            var b = new WeakToStrongBinder();
            var ss = new LegacySurrogateSelector();
            foreach (TypeMap map in maps)
            {
                b.AddMap(map);
                ss.AddSurrogate(new StrongToWeakItemSurrogate(map));
            }

            NewReader.Binder = b;
            NewWriter.SurrogateSelector = ss;
        }

        public BinaryFormatter NewReader { get; private set; }
        public BinaryFormatter NewWriter { get; private set; }
        public BinaryFormatter Old { get; private set; }
    }
}