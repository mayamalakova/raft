using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Raft.Election;
using Raft.Entities;
using Shouldly;

namespace Raft.Test.Election
{
    public class LeaderNodeShould
    {
        private const string TestValue = "test-message";
        
        private LeaderNodeRunner _leaderNodeRunner;
        private IMessageBroker _messageBroker;
        private Node _node;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node("test");
            _leaderNodeRunner = new LeaderNodeRunner(_node, 100, _messageBroker);
        }
        
        [Test]
        public void UpdateLog_OnValueUpdateReceived()
        {
            var message = new NodeMessage(TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _node.Log.Count.ShouldBe(1);
            _node.Log[0].Value.ShouldBe(TestValue);
            _node.Log[0].Type.ShouldBe(OperationType.Update);
        }
        
        [Test]
        public void SendLogUpdate_OnValueUpdateReceived()
        {
            var message = new NodeMessage(TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate));
        }

        [Test]
        public void CommitUpdate_WhenMajorityConfirmsLogUpdate()
        {
            _leaderNodeRunner.NodesCount = 3;
            var updateId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, TestValue, updateId));
            
            var messageA = new NodeMessage(TestValue, MessageType.LogUpdateConfirmation, "A", updateId);
            var messageB = new NodeMessage(TestValue, MessageType.LogUpdateConfirmation, "B", updateId);
            
            _leaderNodeRunner.ReceiveMessage(messageA);
            _node.Log.Last().Type.ShouldBe(OperationType.Update);
            _messageBroker.DidNotReceiveWithAnyArgs().Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
            
            _leaderNodeRunner.ReceiveMessage(messageB);
            _node.Log.Last().Type.ShouldBe(OperationType.Commit);
            _node.Value.ShouldBe(TestValue);
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
        }
        
    }
}