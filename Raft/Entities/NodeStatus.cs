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
        public HashSet<string> ConfirmedNodes { get; } = new HashSet<string>();

        public string Name => NodeStatus.Leader;
        public override string ToString() => "(leader)";
    }
    
    public class FollowerStatus: INodeStatus
    {
        public string Name => NodeStatus.Follower;
        public override string ToString() => "";
    }
    
    public class CandidateStatus: INodeStatus
    {
        public string Name => NodeStatus.Candidate;
        public override string ToString() => "(candidate)";
    }
}