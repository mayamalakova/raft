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
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _leader = new Node("Leader", _messageBroker)
            {
                Status = new LeaderStatus(0)
            };
            _leaderTimerStrategy = new LeaderTimerStrategy(_leader);
        }
        
        [Test]
        public void ResetTimer_WhenMessageFromNewerLeaderReceived()
        {
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
        
        [Test]
        public void Ping_OnTimerElapsed()
        {
            _leaderTimerStrategy.OnTimerElapsed();
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.Info));
        }
    }
}