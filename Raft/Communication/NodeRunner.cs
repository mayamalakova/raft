using System;
using NLog;
using Raft.Entities;
using Raft.NodeStrategy;

namespace Raft.Communication
{
    /// <summary>
    /// Triggers message processing and tracks timeouts
    /// </summary>
    public class NodeRunner : IMessageBrokerListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITimer _timer;

        private readonly StrategySelector _strategySelector;

        private Node Node { get; }
        public string Name => Node.Name;

        public override string ToString()
        {
            return $"{Name} ({Node.Status.Term}) {Node.Status} - {Node.Value}".Replace("  ", " ");
        }

        public NodeRunner(Node node, ITimer timer, StrategySelector strategySelector)
        {
            Node = node;
            _strategySelector = strategySelector;
            _timer = timer;
        }

        public void ReceiveMessage(NodeMessage message)
        {
            if (Node.Name.Equals(message.SenderName))
            {
                return;
            }

            if (ShouldResetTimer(message))
            {
                _timer.Reset();
            }

            RespondToMessage(message);
        }

        private bool ShouldResetTimer(NodeMessage message)
        {
            return _strategySelector.SelectTimerStrategy(Node).ShouldReset(message);
        }

        private void RespondToMessage(NodeMessage message)
        {
            _strategySelector.SelectResponseStrategy(Node).RespondToMessage(message);
        }
        
        public void Start()
        {
            _timer.Start();

            _timer.Elapsed += (sender, args) =>
            {
                _timer.Reset();
                _strategySelector.SelectTimerStrategy(Node).OnTimerElapsed();
            };
        }
    }
}