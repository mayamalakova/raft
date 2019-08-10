using System.Timers;
using NLog;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner: IMessageBrokerListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Timer _timer;
        public IMessageResponseStrategy MessageResponseStrategy { get; set; }
        private Node Node { get; }
        public string Name => Node.Name;

        public override string ToString()
        {
            var nodeStatus = Node.Status == NodeStatus.Leader ? "(leader) " : "";
            return $"{Name} {nodeStatus}- {Node.Value}";
        }

        public NodeRunner(Node node, int electionTimeout)
        {
            Node = node;
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

        private void RespondToMessage(NodeMessage message)
        {
            MessageResponseStrategy.RespondToMessage(message);
        }

        public void Start()
        {
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Logger.Trace($"node {Node.Name} - timer elapsed");
                Node.SendVoteRequest();
                MessageResponseStrategy = new CandidateMessageResponseStrategy(Node);
            };
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
    }
}