using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Communication;
using Raft.Entities;
using Raft.NodeStrategy.Timer;
using Shouldly;

namespace Raft.Test.Strategy.Timer
{
    public class CandidateTimerStrategyShould
    {
        private Node _candidate;
        private ITimerStrategy _candidateTimerStrategy;
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _candidate = new Node("Candidate", _messageBroker)
            {
                Status = new CandidateStatus(1)
            };
            _candidateTimerStrategy = new CandidateTimerStrategy(node: _candidate);
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromNewLeaderReceived()
        {
            var nodeMessage = new NodeMessage(2, "ping", MessageType.Info, "Leader", Guid.Empty);
            var shouldReset = _candidateTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
        
        [Test]
        public void NotResetTimer_WhenMessageFromOldLeaderReceived()
        {
            var nodeMessage = new NodeMessage(0, "ping", MessageType.Info, "Leader", Guid.Empty);
            var shouldReset = _candidateTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeFalse();
        }
        
        [TestCase(1)]
        [TestCase(2)]
        public void NotResetTimer_WhenMessageFromCandidateReceived(int term)
        {
            var nodeMessage = new NodeMessage(term, "new candidate", MessageType.VoteRequest, "anotherCandidate", Guid.Empty);
            var shouldReset = _candidateTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeFalse();
        }
        
        [Test]
        public void ResendVoteRequest_OnTimerElapsed()
        {
            _candidateTimerStrategy.OnTimerElapsed();
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.VoteRequest));
        }
    }
}