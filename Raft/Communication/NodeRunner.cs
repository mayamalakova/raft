using System;
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

            if (Node.Status.Name != NodeStatus.Leader && (FromLeader(message) || message.Type == MessageType.VoteRequest))
            {
                RestartElectionTimeout();
            }

            RespondToMessage(message);
        }

        private static bool FromLeader(NodeMessage message)
        {
            return message.Type == MessageType.Info || message.Type == MessageType.LogUpdate ||
                   message.Type == MessageType.LogCommit;
        }

        private void RespondToMessage(NodeMessage message)
        {
            _strategySelector.SelectStrategy(Node).RespondToMessage(message);
        }

        public void Start()
        {
            _timer.Start();

            _timer.Elapsed += (sender, args) =>
            {
                RestartElectionTimeout();
                switch (Node.Status.Name)
                {
                    case NodeStatus.Leader:
                        Node.SendPing();
                        break;
                    case NodeStatus.Candidate:
                        RequestVote();
                        break;
                    case NodeStatus.Follower:
                        BecomeCandidate();
                        break;
                    default:
                        throw new ArgumentException("Unknown node status");
                }
            };
        }

        private void RequestVote()
        {
            var newTerm = Node.Status.Term + 1;
            Node.Status.Term = newTerm;
            Node.SendVoteRequest(newTerm);
        }

        private void BecomeCandidate()
        {
            var newTerm = Node.Status.Term + 1;
            
            Logger.Debug($"{Node.Name} becomes candidate, term: {newTerm}");
            
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