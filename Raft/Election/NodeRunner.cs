using System;
using System.Timers;
using NLog;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner: IMessageBrokerListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Timer _timer;
        private readonly IMessageResponseStrategy _messageResponseStrategy;
        protected Node Node { get; }
        protected NodeStatus Status { get; set; }
        public string Name => Node.Name;

        public NodeRunner(Node node, int electionTimeout)
        {
            Node = node;
            Status = NodeStatus.Follower;

            _messageResponseStrategy = new FollowerMessageResponseStrategy(node);
            _timer = new Timer(electionTimeout * 10);
        }
        
        public void ReceiveMessage(NodeMessage message)
        {
            if (Node.Name.Equals(message.SenderName))
            {
                return;
            }

            RestartElectionTimeout();
            
            RespondToMessage(message);
        }

        public virtual void DisplayStatus()
        {
            Console.WriteLine($"{Name} - {Node.Value}");
        }

        protected virtual void RespondToMessage(NodeMessage message)
        {
            _messageResponseStrategy.RespondToMessage(message);
        }

        public void Start()
        {
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Logger.Trace($"node {Node.Name} - timer elapsed");
            };
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
    }
}