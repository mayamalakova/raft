using System;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Selects the strategy for responding to Raft messages that a node is using
    /// </summary>
    public class StrategySelector
    {
        private readonly int _count;
        
        public StrategySelector(int count)
        {
            _count = count;
        }

        public IMessageResponseStrategy SelectStrategy(Node node)
        {
            switch (node.Status.Name)
            {
                    case NodeStatus.Follower:
                        return new FollowerStrategy(node);
                    case NodeStatus.Leader:
                        return new LeaderStrategy(node, _count);
                    case NodeStatus.Candidate:
                        return new CandidateStrategy(node, _count);
                    default: throw new ArgumentException("Unknown status");
            }
        }

    }
}