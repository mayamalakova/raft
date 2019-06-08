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
            if (Node.Name.Equals(message.SenderName))
            {
                return;
            }

            RestartElectionTimeout();
            
            switch (message.Type)
            {
                case MessageType.ValueUpdate:
                    Console.WriteLine($"Leader node {Node.Name} got message {message.Value}");
                    var logUpdate = new NodeMessage(message.Value, MessageType.LogUpdate, Node.Name);
                    Broker.Broadcast(logUpdate);
                    break;
                case MessageType.LogUpdateReceived:
                    Console.WriteLine($"node {message.SenderName} confirmed");
                    break;
                case MessageType.LogUpdate:
                    break;
                case MessageType.LogCommit:
                    break;
                case MessageType.Info:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}