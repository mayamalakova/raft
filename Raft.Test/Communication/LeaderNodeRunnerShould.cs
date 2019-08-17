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
    public class LeaderNodeRunnerShould
    {
        private const string TestValue = "test-message";
        
        private NodeRunner _leaderNodeRunner;
        private IMessageBroker _messageBroker;
        private Node _node;
        private ITimer _timer;
        private const string NodeName = "test";

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(NodeName, _messageBroker) {Status = new LeaderStatus(0)};
            _timer = Substitute.For<ITimer>();
            _leaderNodeRunner = new NodeRunner(_node, _timer, new StrategySelector(3));
        }
        
        [Test]
        public void UpdateLog_OnValueUpdateReceived()
        {
            var message = new NodeMessage(0, TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _node.Log.Count.ShouldBe(1);
            _node.Log[0].Value.ShouldBe(TestValue);
            _node.Log[0].Type.ShouldBe(OperationType.Update);
        }
        
        [Test]
        public void SendLogUpdate_OnValueUpdateReceived()
        {
            var message = new NodeMessage(0, TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate));
        }

        [Test]
        public void CommitUpdate_WhenMajorityConfirmsLogUpdate()
        {
            var updateId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, TestValue, updateId));
            
            var messageA = new NodeMessage(0, TestValue, MessageType.LogUpdateConfirmation, "A", updateId);
            var messageB = new NodeMessage(0, TestValue, MessageType.LogUpdateConfirmation, "B", updateId);
            
            _leaderNodeRunner.ReceiveMessage(messageA);
            _node.Log.Last().Type.ShouldBe(OperationType.Update);
            _messageBroker.DidNotReceiveWithAnyArgs().Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
            
            _leaderNodeRunner.ReceiveMessage(messageB);
            _node.Log.Last().Type.ShouldBe(OperationType.Commit);
            _node.Value.ShouldBe(TestValue);
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit));
        }
        
        [Test]
        public void DisplayStatus()
        {
            _leaderNodeRunner.ToString().ShouldBe($"{NodeName} (0) L - {_node.Value}");
        }
        
        [Test]
        public void NotResetTimer_WhenMessageFromLeaderReceived()
        {
            _leaderNodeRunner.ReceiveMessage(new NodeMessage(1, "test", MessageType.Info, "someSender", Guid.Empty));
            
            _timer.Received(0).Reset();
        }
    }
}