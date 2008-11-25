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
namespace MassTransit.Tests.StateMachine
{
    using NUnit.Framework;

    [TestFixture]
    public class LambdaStateName_Specs
    {
        [Test]
        public void I_want_a_state_to_build_itself_properly()
        {
            Assert.IsNotNull(SuperSimpleState.Crazy);
            Assert.AreEqual("Crazy", SuperSimpleState.Crazy.Name);
        }

        [Test]
        public void The_initial_state_should_be_set_via_the_builder()
        {
            Assert.AreEqual(SuperSimpleState.Crazy, SuperSimpleState.Initial);
        }
    }

    internal class SuperSimpleState : StateMachineBase<SuperSimpleState>
    {
        static SuperSimpleState()
        {
            Define(() => Crazy).AsInitial();
            Define(() => Boom);
        }

        public static State<SuperSimpleState> Crazy { get; set; }
        public static StateEvent<SuperSimpleState> Boom { get; set; }
    }
}