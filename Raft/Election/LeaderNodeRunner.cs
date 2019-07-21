using System;
using System.Collections.Generic;
using NLog;
using Raft.Entities;

namespace Raft.Election
{
    public class LeaderNodeRunner : NodeRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<string> _confirmedNodes = new HashSet<string>();
        private readonly IMessageResponseStrategy _messageResponseStrategy;
        public int NodesCount { private get; set; } = 3;

        public LeaderNodeRunner(Node node, int electionTimeout) : base(node,
            electionTimeout)
        {
            Status = NodeStatus.Leader;
            _messageResponseStrategy = new LeaderMessageResponseStrategy(node, NodesCount);
        }

        protected override void RespondToMessage(NodeMessage message)
        {
            _messageResponseStrategy.RespondToMessage(message);
        }

        public override void DisplayStatus()
        {
            Console.WriteLine($"{Name} (leader)- {Node.Value}");
        }
    }
}