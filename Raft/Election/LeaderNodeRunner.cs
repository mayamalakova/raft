using System;

namespace Raft.Election
{
    public class LeaderNodeRunner: NodeRunner
    {
        public LeaderNodeRunner(string name, int electionTimeout, IMessageBroker messageBroker) : base(name, electionTimeout, messageBroker)
        {
            Status = NodeStatus.Leader;    
        }

        public override void ReceiveMessage(NodeMessage message)
        {
            Console.WriteLine($"Leader node {Node.Name} got message {message.Message}");
            RestartElectionTimeout();
        }
    }
}