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
            _node.ResendVoteRequest();
        }

        public bool ShouldReset(NodeMessage message)
        {
            return FromLeader(message) || FromCandidate(message);
        }
    }
}