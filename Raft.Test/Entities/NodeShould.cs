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
    }
}