using NLog;
using Raft.Entities;
using Raft.NodeStrategy;
using Timer = System.Timers.Timer;

namespace Raft.Communication
{
    /// <summary>
    /// Triggers message processing and tracks timeouts
    /// </summary>
    public class NodeRunner : IMessageBrokerListener
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

            if (message.Type == MessageType.Info || message.Type == MessageType.LogUpdate ||
                message.Type == MessageType.LogCommit)
            {
                RestartElectionTimeout();
            }

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
                if (Node.Status.Name == NodeStatus.Follower)
                {
                    BecomeCandidate();
                }
                else if (Node.Status.Name == NodeStatus.Leader)
                {
                    Node.SendPing();
                }
                else
                {
                    var newTerm = Node.Status.Term + 1;
                    Node.Status.Term = newTerm;
                    Node.SendVoteRequest(newTerm);
                }
            };
        }

        private void BecomeCandidate()
        {
            Logger.Debug($"{Node.Name} becomes candidate");
            
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