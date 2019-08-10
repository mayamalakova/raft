using Raft.Entities;

namespace Raft.NodeStrategy
{
    public class CandidateMessageResponseStrategy : FollowerMessageResponseStrategy
    {
        public CandidateMessageResponseStrategy(Node node) : base(node)
        {
        }
    }
}