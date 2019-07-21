using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Election;
using Raft.Entities;
using Shouldly;

namespace Raft.Test.Election
{
    [TestFixture]
    public class NodeRunnerShould
    {
        private const string TestValue = "test-message";
        
        [Test]
        public void UpdateLog_OnLogUpdate()
        {
            var messageBroker = Substitute.For<IMessageBroker>();
            var nodeRunner = new NodeRunner("test", 100, messageBroker);

            var message = new NodeMessage(TestValue, MessageType.LogUpdate, null, Guid.Empty);
            nodeRunner.ReceiveMessage(message);
            
            nodeRunner.Log.Count.ShouldBe(1);
            nodeRunner.Log[0].Value.ShouldBe(TestValue);
            nodeRunner.Log[0].Type.ShouldBe(OperationType.Update);
        }
    }
}