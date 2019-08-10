using Raft.Entities;

namespace Raft.Election
{
    public class CandidateMessageResponseStrategy : FollowerMessageResponseStrategy
    {
        public CandidateMessageResponseStrategy(Node node) : base(node)
        {
        }
    }
}