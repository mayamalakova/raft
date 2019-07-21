using System;
using System.Collections.Generic;
using NLog;
using Raft.Entities;

namespace Raft.Election
{
    public class LeaderNodeRunner: NodeRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly HashSet<string> _confirmedNodes = new HashSet<string>();
        public int NodesCount { get; set;  } = 3;

        public LeaderNodeRunner(Node node, int electionTimeout, IMessageBroker messageBroker) : base(node, electionTimeout, messageBroker)
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
                    SendLogUpdateRequest(message, entryId);
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
                        SendCommit(message);
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

        private void SendCommit(NodeMessage message)
        {
            var nodeMessage = new NodeMessage(message.Value, MessageType.LogCommit, Node.Name, message.Id);
            Broker.Broadcast(nodeMessage);
        }

        private void SendLogUpdateRequest(NodeMessage message, Guid entryId)
        {
            Logger.Debug($"{Node.Name} initiating update {entryId}");
            
            var logUpdate = new NodeMessage(message.Value, MessageType.LogUpdate, Node.Name, entryId);
            Broker.Broadcast(logUpdate);
        }

    }
}