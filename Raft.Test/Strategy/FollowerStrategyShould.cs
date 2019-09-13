using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Strategy
{
    [TestFixture]
    public class FollowerStrategyShould
    {
        private const string FollowerName = "F";
        
        private Node _node;
        private FollowerStrategy _followerStrategy;
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(FollowerName, _messageBroker)
            {
                Status = new FollowerStatus(0)
            };
            _followerStrategy = new FollowerStrategy(_node);
        }

        [Test]
        public void UpdateLogAndConfirm_OnLogUpdate()
        {
            var logUpdate = new NodeMessage(0, "test", MessageType.LogUpdate, "L", Guid.Empty);
            _followerStrategy.RespondToMessage(logUpdate);

            _node.LastLogEntry().Value.ShouldBe("test");
            _node.LastLogEntry().Type.ShouldBe(OperationType.Update);
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(
                m => m.Type == MessageType.LogUpdateConfirmation &&
                     m.SenderName == FollowerName));
        }

        [Test]
        public void CommitLogAndUpdateValue_OnLogCommit()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "test", entryId));
            
            var logUpdate = new NodeMessage(0, "test", MessageType.LogCommit, "L", entryId);
            _followerStrategy.RespondToMessage(logUpdate);
            
            _node.LastLogEntry().Value.ShouldBe("test");
            _node.LastLogEntry().Type.ShouldBe(OperationType.Commit);
        }
                
        [TestCase(MessageType.LogUpdate)]
        [TestCase(MessageType.LogCommit)]
        public void RejectLogReplicationMessagesFromOlderTerms(MessageType messageType)
        {
            _node.Status = new FollowerStatus(2);
            var logUpdate = new NodeMessage(1, "test", messageType, "L", Guid.Empty);
            _followerStrategy.RespondToMessage(logUpdate);

            _node.Log.ShouldBeEmpty();
            _messageBroker.Received(0).Broadcast(Arg.Any<NodeMessage>());
        }

        [Test]
        public void IgnoreLogCommits_ForIrrelevantEntries()
        {
            var entryId = Guid.NewGuid();
            _node.Log.Add(new LogEntry(OperationType.Update, "current entry", entryId));
            
            var logUpdate = new NodeMessage(0, "irrelevant entry", MessageType.LogCommit, "L", Guid.NewGuid());
            _followerStrategy.RespondToMessage(logUpdate);
            
            _node.LastLogEntry().Value.ShouldBe("current entry");
            _node.LastLogEntry().Type.ShouldBe(OperationType.Update);
        }

        [Test]
        public void VoteForLeader_WhenNotVotedInTerm()
        {
            var voteRequest = new NodeMessage(1, "new candidate", MessageType.VoteRequest, "C", Guid.NewGuid());
            _followerStrategy.RespondToMessage(voteRequest);
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(
                m => m.Type == MessageType.LeaderVote && m.SenderName == FollowerName));
        }
        
        [Test]
        public void NotVoteForLeader_WhenAlreadyVotedInTerm()
        {
            var voteRequest = new NodeMessage(0, "new candidate", MessageType.VoteRequest, "C", Guid.NewGuid());
            _followerStrategy.RespondToMessage(voteRequest);
            
            _messageBroker.Received(0).Broadcast(Arg.Any<NodeMessage>());
        }

        
        
    }
}