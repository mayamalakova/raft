namespace Raft.Election
{
    public class LeaderNodeRunner: NodeRunner
    {
        public LeaderNodeRunner(string name, int electionTimeout) : base(name, electionTimeout)
        {
            Status = NodeStatus.Leader;    
        }
    }
}