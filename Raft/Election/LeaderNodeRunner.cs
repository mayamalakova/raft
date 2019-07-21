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
        public int NodesCount { private get; set; } = 3;

        public LeaderNodeRunner(Node node, int electionTimeout) : base(node,
            electionTimeout)
        {
            Status = NodeStatus.Leader;
        }

        protected override void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ValueUpdate:
                    Logger.Debug($"Leader {Node.Name} got value {message.Value}");

                    var entryId = Guid.NewGuid();
                    Node.UpdateLog(message, entryId);
                    Node.SendLogUpdateRequest(message, entryId);
                    break;

                case MessageType.LogUpdateConfirmation:
                    if (message.Id != Node.LastLogEntry().Id)
                    {
                        return;
                    }

                    _confirmedNodes.Add(message.SenderName);
                    if (_confirmedNodes.Count > NodesCount / 2)
                    {
                        Node.CommitLog(message);
                        Node.SendCommit(message);
                    }

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

        public override void DisplayStatus()
        {
            Console.WriteLine($"{Name} (leader)- {Node.Value}");
        }
    }
}