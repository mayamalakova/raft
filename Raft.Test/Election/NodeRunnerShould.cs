using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Raft.Election;
using Raft.Entities;
using Shouldly;

namespace Raft.Test.Election
{
    public class NodeRunnerShould
    {
        private const string TestValue = "test-message";
        
        private NodeRunner _nodeRunner;
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _nodeRunner = new NodeRunner("test", 100, _messageBroker);
        }
        
        [Test]
        public void UpdateLogAndConfirm_OnLogUpdate()
        {
            var message = new NodeMessage(TestValue, MessageType.LogUpdate, null, Guid.Empty);
            _nodeRunner.ReceiveMessage(message);
            
            _nodeRunner.Log.Count.ShouldBe(1);
            _nodeRunner.Log[0].Value.ShouldBe(TestValue);
            _nodeRunner.Log[0].Type.ShouldBe(OperationType.Update);
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdateConfirmation));
        }

        [Test]
        public void Commit_OnLogCommit()
        {
            var updateId = Guid.NewGuid();
            _nodeRunner.Log.Add(new LogEntry(OperationType.Update, TestValue, updateId));
            
            var message = new NodeMessage(TestValue, MessageType.LogCommit, null, updateId);
            _nodeRunner.ReceiveMessage(message);
            
            _nodeRunner.Log.Last().Type.ShouldBe(OperationType.Commit);
            _nodeRunner.Node.Value.ShouldBe(TestValue);
        }
    }
}