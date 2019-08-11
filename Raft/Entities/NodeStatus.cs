using System.Collections.Generic;

namespace Raft.Entities
{

    public static class NodeStatus
    {
        public const string Leader = "leader";
        public const string Candidate = "candidate";
        public const string Follower = "follower";
    }
    
    public class LeaderStatus: INodeStatus
    {
        public LeaderStatus(int term)
        {
            Term = term;
        }

        public HashSet<string> ConfirmedNodes { get; } = new HashSet<string>();

        public string Name => NodeStatus.Leader;
        
        public int Term { get; set; }
        public override string ToString() => "L";
    }
    
    public class FollowerStatus: INodeStatus
    {
        public FollowerStatus(int term)
        {
            Term = term;
        }

        public string Name => NodeStatus.Follower;
        
        public int Term { get; set; }
        public override string ToString() => "F";
    }
    
    public class CandidateStatus: INodeStatus
    {
        public CandidateStatus(int term)
        {
            Term = term;
        }

        public HashSet<string> ConfirmedNodes { get; } = new HashSet<string>();
        
        public string Name => NodeStatus.Candidate;
        
        public int Term { get; set; }
        public override string ToString() => "C";
    }
}