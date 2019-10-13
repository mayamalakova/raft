using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Shouldly;

namespace Raft.Test.Entities
{
    [TestFixture]
    public class NodeShould
    {
        private Node _node;
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node("A", _messageBroker) {Status = new FollowerStatus(0)};
        }

        [Test]
        public void IncreaseTerm_WhenResendingVoteRequest()
        {
            _node.ResendVoteRequest();
            
            _node.Status.Term.ShouldBe(1);
        }

        [Test]
        public void IncreaseTerm_WhenBecomingCandidate()
        {
            _node.BecomeCandidate();
            
            _node.Status.Term.ShouldBe(1);
            _node.Status.Name.ShouldBe(NodeStatus.Candidate);
                
        }

        [Test]
        public void SendVoteRequest_WhenBecomingCandidate()
        {
            _node.BecomeCandidate();
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.VoteRequest));
        }

        [Test]
        public void SendLastLogEntryTermAndIndexWithVoteRequest()
        {
            _node.Log.Add(new LogEntry(OperationType.Update, "value", Guid.NewGuid(), 1));
            _node.BecomeCandidate();
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.VoteRequest && m.Value.Equals("1,0")));
        }

        [Test]
        public void VoteForItself_WhenBecomingCandidate()
        {
            _node.BecomeCandidate();
            
            ((CandidateStatus) _node.Status).ConfirmedNodes.ShouldContain(_node.Name);
        }
    }
}