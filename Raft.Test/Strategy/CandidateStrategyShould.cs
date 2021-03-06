using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Strategy
{
    [TestFixture]
    public class CandidateStrategyShould
    {
        private const int CandidateTerm = 1;
        private const string CandidateName = "test";
        private IMessageBroker _messageBroker;
        private Node _node;
        private CandidateStrategy _candidateStrategy;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(CandidateName, _messageBroker) {Status = new CandidateStatus(CandidateTerm)};
            _candidateStrategy = new CandidateStrategy(_node, 3);
        }

        [Test]
        public void AddVotes_OnVoteReceived()
        {
            var vote = new NodeMessage(CandidateTerm, CandidateName, MessageType.LeaderVote, "A", Guid.Empty);
            _candidateStrategy.RespondToMessage(vote);

            var candidateStatus = _node.Status as CandidateStatus;
            candidateStatus.ShouldNotBeNull();
            candidateStatus.ConfirmedNodes.Count.ShouldBe(CandidateTerm);
        }

        [Test]
        public void BecomeLeader_OnMajorityOfVotesReceived()
        {
            var voteA = new NodeMessage(CandidateTerm, CandidateName, MessageType.LeaderVote, "A", Guid.Empty);
            _candidateStrategy.RespondToMessage(voteA);
            var voteB = new NodeMessage(CandidateTerm, CandidateName, MessageType.LeaderVote, "B", Guid.Empty);
            _candidateStrategy.RespondToMessage(voteB);

            _node.Status.Name.ShouldBe(NodeStatus.Leader);
        }

        [Test]
        public void SendPing_OnBecomingLeader()
        {
            var voteA = new NodeMessage(CandidateTerm, CandidateName, MessageType.LeaderVote, "A", Guid.Empty);
            _candidateStrategy.RespondToMessage(voteA);
            var voteB = new NodeMessage(CandidateTerm, CandidateName, MessageType.LeaderVote, "B", Guid.Empty);
            _candidateStrategy.RespondToMessage(voteB);

            _node.Status.Name.ShouldBe(NodeStatus.Leader);
            _messageBroker.Received(1)
                .Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.Info && m.SenderName == CandidateName));
        }

        [TestCase(MessageType.LogUpdate)]
        [TestCase(MessageType.LogCommit)]
        [TestCase(MessageType.Info)]
        public void IgnoreOldLeaderMessages(MessageType messageType)
        {
            var fromLeader = new NodeMessage(0, CandidateName, messageType, "L", Guid.Empty);
            _candidateStrategy.RespondToMessage(fromLeader);

            _node.Status.Name.ShouldBe(NodeStatus.Candidate);
        }

        [TestCase(MessageType.LogUpdate)]
        [TestCase(MessageType.LogCommit)]
        [TestCase(MessageType.Info)]
        public void BecomeFollower_OnNewTermLeaderFound(MessageType messageType)
        {
            var fromLeader = new NodeMessage(2, CandidateName, messageType, "L", Guid.Empty);
            _candidateStrategy.RespondToMessage(fromLeader);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);
        }

        [Test]
        public void BecomeFollowerAndUpdateLog_OnLogUpdateFromNewLeader()
        {
            var fromLeader = new NodeMessage(2, "new value", MessageType.LogUpdate, "L", Guid.Empty);
            _candidateStrategy.RespondToMessage(fromLeader);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);
            _node.Log.Count.ShouldBe(1);
            _node.LastLogEntry().Value.ShouldBe("new value");
            _node.LastLogEntry().Type.ShouldBe(OperationType.Update);
        }

        [Test]
        public void BecomeFollowerAndCommitLog_OnLogCommitFromNewLeader()
        {
            var entryId = Guid.Empty;
            _node.Log.Add(new LogEntry(OperationType.Update, "new value", entryId, 2));
            var fromLeader = new NodeMessage(2, "new value", MessageType.LogCommit, "L", entryId);
            _candidateStrategy.RespondToMessage(fromLeader);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);
            _node.Log.Count.ShouldBe(1);
            _node.LastLogEntry().Value.ShouldBe("new value");
            _node.LastLogEntry().Type.ShouldBe(OperationType.Commit);
        }

        [Test]
        public void IgnoreClientRequests()
        {
            var valueUpdate = new NodeMessage(0, CandidateName, MessageType.ValueUpdate, null, Guid.Empty);
            _candidateStrategy.RespondToMessage(valueUpdate);

            _node.Status.Name.ShouldBe(NodeStatus.Candidate);
            _messageBroker.Received(0).Broadcast(Arg.Any<NodeMessage>());
        }

        [Test]
        public void BecomeFollowerAndVote_OnNewerTermStarted()
        {
            var newCandidateName = "C";
            var voteRequest =
                new NodeMessage(CandidateTerm + 1, CandidateName, MessageType.VoteRequest, newCandidateName, Guid.Empty);
            _candidateStrategy.RespondToMessage(voteRequest);

            _node.Status.Name.ShouldBe(NodeStatus.Follower);

            _messageBroker.Received(1)
                .Send(Arg.Is<NodeMessage>(m => m.Type == MessageType.LeaderVote && m.SenderName == CandidateName),
                    Arg.Is<string>(x => x.Equals(newCandidateName)));
        }

        [Test]
        public void IgnoreStaleMessages()
        {
            var voteRequest =
                new NodeMessage(CandidateTerm - 1, CandidateName, MessageType.VoteRequest, "C", Guid.Empty);
            _candidateStrategy.RespondToMessage(voteRequest);

            _node.Status.Name.ShouldBe(NodeStatus.Candidate);

            _messageBroker.DidNotReceiveWithAnyArgs()
                .Broadcast(message: Arg.Any<NodeMessage>());
        }
    }
}