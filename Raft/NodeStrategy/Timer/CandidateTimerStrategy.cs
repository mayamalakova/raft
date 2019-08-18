using System;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy.Timer
{
    public class CandidateTimerStrategy : BaseTimerStrategy, ITimerStrategy 
    {
        private readonly Node _node;

        public CandidateTimerStrategy(Node node)
        {
            _node = node;
        }

        public void OnTimerElapsed()
        {
            throw new NotImplementedException();
        }

        public bool ShouldReset(NodeMessage message)
        {
            return FromLeader(message) || FromCandidate(message);
        }
    }
}