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
            if (!Node.Name.Equals(message.SenderName))
            {
                Console.WriteLine($"Leader node {Node.Name} got message {message.Message}");
                var logUpdate = new NodeMessage(message.Message, true, MessageType.LogUpdate, Node.Name);
                Broker.Broadcast(logUpdate);
                RestartElectionTimeout();
            }
        }
    }
}