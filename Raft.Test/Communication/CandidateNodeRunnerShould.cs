using NSubstitute;
using NUnit.Framework;
using Raft.Communication;
using Raft.Entities;
using Raft.NodeStrategy;
using Shouldly;

namespace Raft.Test.Communication
{
    [TestFixture]
    public class CandidateNodeRunnerShould
    {
        private NodeRunner _candidate;

        private IMessageBroker _messageBroker;
        private Node _node;
        private const string NodeName = "test";

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _node = new Node(NodeName, _messageBroker) {Status = new CandidateStatus(1)};
            _candidate = new NodeRunner(_node, 100, new StrategySelector(3));
        }
        
        [Test]
        public void DisplayStatus()
        {
            _candidate.ToString().ShouldBe($"{NodeName} (1) C - {_node.Value}");
        }
    }
}