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
        private IMessageBroker _messageBroker;
        private Node _node;
        private CandidateStrategy _candidateStrategy;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node("test", _messageBroker) {Status = new CandidateStatus(1)};
            _candidateStrategy = new CandidateStrategy(_node, 3,1 );
        }
        
        [Test]
        public void AddVotes_OnVoteReceived()
        {
            var vote = new NodeMessage(1, "test", MessageType.LeaderVote, "test", Guid.Empty);
            _candidateStrategy.RespondToMessage(vote);

            var candidateStatus = _node.Status as CandidateStatus;
            candidateStatus.ShouldNotBeNull();
            candidateStatus.ConfirmedNodes.Count.ShouldBe(1);
        }
    }
}