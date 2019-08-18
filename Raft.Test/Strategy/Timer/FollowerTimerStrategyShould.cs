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
        private Node _follower;
        private FollowerTimerStrategy _followerTimerStrategy;

        [SetUp]
        public void SetUp()
        {
            _follower = new Node("Follower", Substitute.For<IMessageBroker>())
            {
                Status = new FollowerStatus(0)
            };
            _followerTimerStrategy = new FollowerTimerStrategy(node: _follower);

        }
        
        [Test]
        public void ResetTimer_WhenMessageFromLeaderReceived()
        {
            var nodeMessage = new NodeMessage(0, "ping", MessageType.Info, "Leader", Guid.Empty);
            var shouldReset = _followerTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromCandidateReceived()
        {
            var nodeMessage = new NodeMessage(1, "from candidate", MessageType.VoteRequest, "candidate", Guid.Empty);
            var shouldReset = _followerTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
        
        [Test]
        public void BecomeCandidate_OnTimerElapsed()
        {
            _followerTimerStrategy.OnTimerElapsed();
            
            _follower.Status.Name.ShouldBe(NodeStatus.Candidate);      
        }
    }
}