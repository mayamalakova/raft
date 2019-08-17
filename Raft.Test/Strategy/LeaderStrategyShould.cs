using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Strategy
{
    [TestFixture]
    public class LeaderStrategyShould
    {
        private IMessageBroker _messageBroker;
        private LeaderStrategy _leaderStrategy;
        private Node _node;


        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node("L", _messageBroker) {Status = new LeaderStatus(0)};
            _leaderStrategy = new LeaderStrategy(_node, 3);
        }
        
        [Test]
        public void AskForLogUpdate_WhenClientRequestsValueUpdate()
        {
            var valueUpdate = new NodeMessage(0, "new value", MessageType.ValueUpdate, null, Guid.Empty);
            _leaderStrategy.RespondToMessage(valueUpdate);
            
            _messageBroker.Received(1).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate && 
                m.SenderName == "L"));
        }

        [Test]
        public void AddConfirmations_OnLogUpdateConfirmationReceived()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId));
            var updateConfirmation = new NodeMessage(0, "new value", MessageType.LogUpdateConfirmation, "A", entryId);
            
            _leaderStrategy.RespondToMessage(updateConfirmation);
            
            var leaderStatus = _node.Status as LeaderStatus;
            leaderStatus?.ConfirmedNodes.Count.ShouldBe(1);
        }

        [Test]
        public void CommitLogAndUpdateValue_OnMajorityConfirmed()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId));
            
            var confirmationA = new NodeMessage(0, "new value", MessageType.LogUpdateConfirmation, "A", entryId);
            _leaderStrategy.RespondToMessage(confirmationA);
            var confirmationB = new NodeMessage(0, "new value", MessageType.LogUpdateConfirmation, "B", entryId);
            _leaderStrategy.RespondToMessage(confirmationB);

            _node.LastLogEntry().Type.ShouldBe(OperationType.Commit);
            _node.LastLogEntry().Value.ShouldBe("new value");
            _node.Value.ShouldBe("new value");
            _messageBroker.Received(1).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit && m.SenderName == "L"));
        }

        [Test]
        public void ResetConfirmations_OnNewValueUpdate()
        {
            var leaderStatus = _node.Status as LeaderStatus;
            leaderStatus?.ConfirmedNodes.Add("A");
            
            var valueUpdate = new NodeMessage(0, "new value", MessageType.ValueUpdate, null, Guid.Empty);
            _leaderStrategy.RespondToMessage(valueUpdate);
            
            leaderStatus?.ConfirmedNodes.ShouldBeEmpty();
        }

        [TestCase(MessageType.LogUpdate)]
        [TestCase(MessageType.LogCommit)]
        [TestCase(MessageType.Info)]
        public void BecomeFollower_OnNewerLeaderFound(MessageType messageType)
        {
            var fromLeader = new NodeMessage(1, "L1", messageType, "L1", Guid.Empty);
            _leaderStrategy.RespondToMessage(fromLeader);
            
            _node.Status.Name.ShouldBe(NodeStatus.Follower);
        }

        [Test]
        public void Ping_OnTimerElapsed()
        {
            _leaderStrategy.OnTimerElapsed();
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.Info));
        }

    }
}