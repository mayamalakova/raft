using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy.Timer;
using Shouldly;

namespace Raft.Test.Strategy.Timer
{
    [TestFixture]
    public class FollowerTimerStrategyShould
    {
        private const int OlderTerm = 0;
        private const int CurrentTerm = 1;
        private const int NewerTerm = 2;

        private Node _follower;
        private FollowerTimerStrategy _followerTimerStrategy;

        [SetUp]
        public void SetUp()
        {
            _follower = new Node("Follower", Substitute.For<IMessageBroker>())
            {
                Status = new FollowerStatus(CurrentTerm)
            };
            _followerTimerStrategy = new FollowerTimerStrategy(node: _follower);

        }
        
        [TestCase(CurrentTerm, true)]
        [TestCase(OlderTerm, false)]
        public void ResetTimer_WhenMessageFromCurrentLeaderReceived(int term, bool expected)
        {
            var nodeMessage = new NodeMessage(term, "ping", MessageType.Info, "Leader", Guid.Empty);
            var shouldReset = _followerTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBe(expected);
        }
        
        [Test]
        public void NotResetTimer_WhenMessageFromCandidateReceived()
        {
            var nodeMessage = new NodeMessage(NewerTerm, "from candidate", MessageType.VoteRequest, "candidate", Guid.Empty);
            var shouldReset = _followerTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeFalse();
        }
        
        [Test]
        public void BecomeCandidate_OnTimerElapsed()
        {
            _followerTimerStrategy.OnTimerElapsed();
            
            _follower.Status.Name.ShouldBe(NodeStatus.Candidate);      
        }
    }
}