using System.Timers;
using NLog;
using Raft.Entities;
using Raft.NodeStrategy;

namespace Raft.Communication
{
    public class NodeRunner: IMessageBrokerListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Timer _timer;

        private readonly StrategySelector _strategySelector;
        
        private Node Node { get; }
        public string Name => Node.Name;
        public bool IsLeading => Node.Status.Name == NodeStatus.Leader;
        public int Term => Node.Status.Term;

        public override string ToString()
        {
            return $"{Name} ({Node.Status.Term}) {Node.Status} - {Node.Value}".Replace("  ", " ");
        }

        public NodeRunner(Node node, int electionTimeout, StrategySelector strategySelector)
        {
            Node = node;
            _strategySelector = strategySelector;
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
            _strategySelector.SelectStrategy(Node).RespondToMessage(message);
        }

        public void Start()
        {
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Logger.Trace($"node {Node.Name} - timer elapsed");
                BecomeCandidate();
            };
        }

        private void BecomeCandidate()
        {
            var newTerm = Node.Status.Term + 1;
            Node.Status = new CandidateStatus(newTerm);
            Node.SendVoteRequest(newTerm);
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
    }
}