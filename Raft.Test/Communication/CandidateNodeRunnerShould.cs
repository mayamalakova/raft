using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Communication;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Communication
{
    [TestFixture]
    public class CandidateNodeRunnerShould
    {
        private NodeRunner _candidate;

        private IMessageBroker _messageBroker;
        private Node _node;
        private ITimer _timer;
        private const string NodeName = "test";

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(NodeName, _messageBroker) {Status = new CandidateStatus(1)};
            _timer = Substitute.For<ITimer>();
            _candidate = new NodeRunner(_node, _timer, new StrategySelector(3));
        }
        
        [Test]
        public void DisplayStatus()
        {
            _candidate.ToString().ShouldBe($"{NodeName} (1) C - {_node.Value}");
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromLeaderReceived()
        {
            _candidate.ReceiveMessage(new NodeMessage(1, "test", MessageType.Info, "someSender", Guid.Empty));
            
            _timer.Received(1).Reset();
        }
        
        [Test]
        public void NotResetTimer_WhenMessageIsFromItself()
        {
            _candidate.ReceiveMessage(new NodeMessage(1, "test", MessageType.Info, NodeName, Guid.Empty));
            
            _timer.Received(0).Reset();
        }
    }
}