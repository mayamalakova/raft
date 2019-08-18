using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy.Timer
{
    public class LeaderTimerStrategy : BaseTimerStrategy, ITimerStrategy
    {
        private readonly Node _node;

        public LeaderTimerStrategy(Node node)
        {
            _node = node;
        }

        public void OnTimerElapsed()
        {
            _node.SendPing();
        }

        public bool ShouldReset(NodeMessage message)
        {
            return FromLeader(message) && message.Term > _node.Status.Term;
        }
    }
}