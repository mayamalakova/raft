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

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _leaderNodeRunner = new LeaderNodeRunner("test", 100, _messageBroker);
        }
        
        [Test]
        public void UpdateLog_OnValueUpdateReceived()
        {
            var message = new NodeMessage(TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _leaderNodeRunner.Log.Count.ShouldBe(1);
            _leaderNodeRunner.Log[0].Value.ShouldBe(TestValue);
            _leaderNodeRunner.Log[0].Type.ShouldBe(OperationType.Update);
            
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
            _leaderNodeRunner.Log.Add(new LogEntry(OperationType.Update, TestValue, updateId));
            
            var messageA = new NodeMessage(TestValue, MessageType.LogUpdateConfirmation, "A", updateId);
            var messageB = new NodeMessage(TestValue, MessageType.LogUpdateConfirmation, "B", updateId);
            
            _leaderNodeRunner.ReceiveMessage(messageA);
            _leaderNodeRunner.Log.Last().Type.ShouldBe(OperationType.Update);
            _messageBroker.DidNotReceiveWithAnyArgs().Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
            
            _leaderNodeRunner.ReceiveMessage(messageB);
            _leaderNodeRunner.Log.Last().Type.ShouldBe(OperationType.Commit);
            _leaderNodeRunner.Node.Value.ShouldBe(TestValue);
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
        }
        
    }
}