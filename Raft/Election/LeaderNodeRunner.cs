using System;
using System.Collections.Generic;
using NLog;

namespace Raft.Election
{
    public class LeaderNodeRunner: NodeRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly HashSet<string> _confirmedNodes = new HashSet<string>();
        private const int NodesCount = 3;

        public LeaderNodeRunner(string name, int electionTimeout, IMessageBroker messageBroker) : base(name, electionTimeout, messageBroker)
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
                    UpdateLog(message, entryId);
                    SendLogUpdateRequest(message, entryId);
                    break;

                case MessageType.LogUpdateReceived:
                    if (message.Id != LastLogEntry().Id)
                    {
                        return;
                    }

                    _confirmedNodes.Add(message.SenderName);
                    if (_confirmedNodes.Count > NodesCount / 2)
                    {
                        CommitLog(message);
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