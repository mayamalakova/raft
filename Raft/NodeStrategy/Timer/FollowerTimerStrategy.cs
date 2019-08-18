using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy.Timer
{
    public class FollowerTimerStrategy : BaseTimerStrategy, ITimerStrategy
    {
        private readonly Node _node;

        public FollowerTimerStrategy(Node node)
        {
            _node = node;
        }

        public void OnTimerElapsed()
        {
            _node.BecomeCandidate();
        }

        public bool ShouldReset(NodeMessage message)
        {
            return FromLeader(message) || FromCandidate(message);
        }
    }
}