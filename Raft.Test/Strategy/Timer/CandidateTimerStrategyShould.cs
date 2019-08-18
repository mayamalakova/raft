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

        [SetUp]
        public void SetUp()
        {
            _candidate = new Node("Candidate", Substitute.For<IMessageBroker>());
            _candidateTimerStrategy = new CandidateTimerStrategy(node: _candidate);
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromLeaderReceived()
        {
            _candidate.Status = new CandidateStatus(1);

            var nodeMessage = new NodeMessage(0, "ping", MessageType.Info, "Leader", Guid.Empty);
            var shouldReset = _candidateTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromAnotherCandidateReceived()
        {
            _candidate.Status = new CandidateStatus(1);

            var nodeMessage = new NodeMessage(2, "new candidate", MessageType.VoteRequest, "anotherCandidate", Guid.Empty);
            var shouldReset = _candidateTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
    }
}