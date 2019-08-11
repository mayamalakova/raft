using NUnit.Framework;
using Raft.Runner;
using Shouldly;

namespace Raft.Test.Runner
{
    [TestFixture]
    public class RaftRunnerShould
    {
        private RaftRunner _raftRunner;

        [SetUp]
        public void SetUp()
        {
            _raftRunner = new RaftRunner();
        }

        [Test]
        public void RegisterFollowersWithMessageBroker()
        {
            var follower = _raftRunner.InitializeFollower("A");
            follower.Name.ShouldBe("A");
            _raftRunner.Broker.Listeners.ShouldContain(x => x.Name.Equals("A"));
        }
        
        [Test]
        public void RegisterLeaderWithMessageBroker()
        {
            var follower = _raftRunner.InitializeLeader("L");
            follower.Name.ShouldBe("L");
            _raftRunner.Broker.Listeners.ShouldContain(x => x.Name.Equals("L"));
        }
    }
}