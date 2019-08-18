using System;
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
            throw new NotImplementedException();
        }

        public bool ShouldReset(NodeMessage message)
        {
            return FromLeader(message) && message.Term > _node.Status.Term;
        }
    }
}