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
            _node = new Node("L", _messageBroker) {Status = new LeaderStatus(1)};
            _leaderStrategy = new LeaderStrategy(_node, 3);
        }

        [Test]
        public void AskForLogUpdate_WhenClientRequestsValueUpdate()
        {
            var valueUpdate = new NodeMessage(1, "new value", MessageType.ValueUpdate, null, Guid.Empty);
            _leaderStrategy.RespondToMessage(valueUpdate);

            _messageBroker.Received(1).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate &&
                                         m.SenderName == "L"));
        }

        [Test]
        public void AddConfirmations_OnLogUpdateConfirmationReceived()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId, 1));
            var updateConfirmation = new NodeMessage(1, "new value", MessageType.LogUpdateConfirmation, "A", entryId);

            _leaderStrategy.RespondToMessage(updateConfirmation);

            var leaderStatus = _node.Status as LeaderStatus;
            leaderStatus?.ConfirmedNodes.Count.ShouldBe(1);
        }

        [Test]
        public void NotCommitLogOrUpdateValue_BeforeMajorityConfirmed()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId, 1));

            var confirmationA = new NodeMessage(1, "new value", MessageType.LogUpdateConfirmation, "A", entryId);
            _leaderStrategy.RespondToMessage(confirmationA);

            _node.LastLogEntry().Type.ShouldBe(OperationType.Update);
            _node.Value.ShouldBe(null);
            _messageBroker.Received(0).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit && m.SenderName == "L"));
        }

        [Test]
        public void CommitLogAndUpdateValue_OnMajorityConfirmed()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId, 1));

            var confirmationA = new NodeMessage(1, "new value", MessageType.LogUpdateConfirmation, "A", entryId);
            _leaderStrategy.RespondToMessage(confirmationA);
            var confirmationB = new NodeMessage(1, "new value", MessageType.LogUpdateConfirmation, "B", entryId);
            _leaderStrategy.RespondToMessage(confirmationB);

            _node.LastLogEntry().Type.ShouldBe(OperationType.Commit);
            _node.Value.ShouldBe("new value");
            _messageBroker.Received(1).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogCommit && m.SenderName == "L"));
        }

        [Test]
        public void ResetConfirmationsAndConfirmNewValue_OnNewValueUpdate()
        {
            var leaderStatus = _node.Status as LeaderStatus;
            leaderStatus?.ConfirmedNodes.Add("A");
            leaderStatus?.ConfirmedNodes.Add("L");

            var valueUpdate = new NodeMessage(1, "new value", MessageType.ValueUpdate, null, Guid.Empty);
            _leaderStrategy.RespondToMessage(valueUpdate);

            leaderStatus?.ConfirmedNodes.Count.ShouldBe(1);
            leaderStatus?.ConfirmedNodes.ShouldContain("L");
        }

        [TestCase(MessageType.LogUpdate)]
        [TestCase(MessageType.LogCommit)]
        [TestCase(MessageType.Info)]
        public void BecomeFollower_OnNewerLeaderFound(MessageType messageType)
        {
            var fromLeader = new NodeMessage(2, "L1", messageType, "L1", Guid.Empty);
            _leaderStrategy.RespondToMessage(fromLeader);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);
        }

        [Test]
        public void BecomeFollowerAndVote_OnNewTermElectionStarted()
        {
            const string newCandidateName = "C1";
            var fromCandidate = new NodeMessage(2, "L1", MessageType.VoteRequest, newCandidateName, Guid.Empty);
            _leaderStrategy.RespondToMessage(fromCandidate);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);
            _messageBroker.Received(1).Send(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LeaderVote && m.SenderName == "L"),
                Arg.Is<string>(x => x.Equals(newCandidateName)));
        }
        
        [TestCase(0, 2, false)]
        [TestCase(2, -1, false)]
        [TestCase(2, 1, true)]
        public void NotBecomeFollower_OnNewTermElectionStartedFromCandidateWithOldLog(int term, int lastLogIndex, bool expectation)
        {
            _node.Log.Add(new LogEntry(OperationType.Update, "some value", Guid.NewGuid(), 2));
            
            const string newCandidateName = "C1";
            var fromCandidate = new NodeMessage(2, $"{term},{lastLogIndex}", MessageType.VoteRequest, newCandidateName, Guid.Empty);
            
            _leaderStrategy.RespondToMessage(fromCandidate);

            _node.Status.Name.ShouldBe(expectation ? NodeStatus.Follower : NodeStatus.Leader);
        }

        [TestCase(MessageType.VoteRequest)]
        [TestCase(MessageType.LogUpdateConfirmation)]
        [TestCase(MessageType.LeaderVote)]
        public void IgnoreStaleServerMessages(MessageType messageType)
        {
            var fromCandidate = new NodeMessage(0, "L1", messageType, "C1", Guid.Empty);
            _leaderStrategy.RespondToMessage(fromCandidate);

            _node.Status.Name.ShouldBe(NodeStatus.Leader);
            _messageBroker.DidNotReceiveWithAnyArgs().Broadcast(Arg.Any<NodeMessage>());
        }

        [Test]
        public void NotIgnoreClientMessages()
        {
            var fromCandidate = new NodeMessage(0, "L1", MessageType.ValueUpdate, "C1", Guid.Empty);
            _leaderStrategy.RespondToMessage(fromCandidate);

            _node.Status.Name.ShouldBe(NodeStatus.Leader);
            _messageBroker.Received(1).Broadcast(Arg.Any<NodeMessage>());
        }
    }
}