using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Raft.Communication;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Communication
{
    public class FollowerNodeRunnerShould
    {
        private const string TestValue = "test-message";
        private const string TestName = "test";

        private NodeRunner _nodeRunner;
        private IMessageBroker _messageBroker;
        private Node _node;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(TestName, _messageBroker) {Status = new FollowerStatus(0)};
            _nodeRunner = new NodeRunner(_node, 100, new StrategySelector(3));
        }
        
        [Test]
        public void UpdateLogAndConfirm_OnLogUpdate()
        {
            var message = new NodeMessage(0, TestValue, MessageType.LogUpdate, null, Guid.Empty);
            _nodeRunner.ReceiveMessage(message);
            
            _node.Log.Count.ShouldBe(1);
            _node.Log[0].Value.ShouldBe(TestValue);
            _node.Log[0].Type.ShouldBe(OperationType.Update);
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdateConfirmation));
        }

        [Test]
        public void Commit_OnLogCommit()
        {
            var updateId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, TestValue, updateId));
            
            var message = new NodeMessage(0, TestValue, MessageType.LogCommit, null, updateId);
            _nodeRunner.ReceiveMessage(message);
            
            _node.Log.Last().Type.ShouldBe(OperationType.Commit);
            _node.Value.ShouldBe(TestValue);
        }

        [Test]
        public void DisplayStatus()
        {
            _nodeRunner.ToString().ShouldBe($"{TestName} (0) F - ");
        }
    }
}