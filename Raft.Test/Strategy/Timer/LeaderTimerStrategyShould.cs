using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy.Timer;
using Shouldly;

namespace Raft.Test.Strategy.Timer
{
    [TestFixture]
    public class LeaderTimerStrategyShould
    {
        private Node _leader;
        private LeaderTimerStrategy _leaderTimerStrategy;

        [SetUp]
        public void SetUp()
        {
            _leader = new Node("Leader", Substitute.For<IMessageBroker>());
            _leaderTimerStrategy = new LeaderTimerStrategy(_leader);
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromNewerLeaderReceived()
        {
            _leader.Status = new LeaderStatus(0);

            var nodeMessage = new NodeMessage(1, "ping", MessageType.Info, "anotherLeader", Guid.Empty);
            var shouldReset = _leaderTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeTrue();
        }
        
        [Test]
        public void NotResetTimer_WhenMessageFromOlderLeaderReceived()
        {
            _leader.Status = new LeaderStatus(1);

            var nodeMessage = new NodeMessage(0, "ping", MessageType.Info, "anotherLeader", Guid.Empty);
            var shouldReset = _leaderTimerStrategy.ShouldReset(nodeMessage);
            
            shouldReset.ShouldBeFalse();
        }
    }
}