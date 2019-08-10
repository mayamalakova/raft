using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Raft.Election;
using Raft.Entities;

namespace Raft.NodeStrategy
{
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
                        return new FollowerMessageResponseStrategy(node);
                    case NodeStatus.Leader:
                        return new LeaderMessageResponseStrategy(node, _count);
                    case NodeStatus.Candidate:
                        return new CandidateMessageResponseStrategy(node);
                    default: throw new ArgumentException("Unknown status");
            }
        }

    }
}