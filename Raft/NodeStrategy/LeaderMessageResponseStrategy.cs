using System;
using System.Collections.Generic;
using Raft.Election;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    public class LeaderMessageResponseStrategy: IMessageResponseStrategy
    {
        private readonly int _nodesCount;
        private readonly HashSet<string> _confirmedNodes = new HashSet<string>();
        private Node Node { get; }
        

        public LeaderMessageResponseStrategy(Node node, int nodesCount)
        {
            _nodesCount = nodesCount;
            Node = node;
        }
        
        public void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ValueUpdate:
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
                    if (_confirmedNodes.Count > _nodesCount / 2)
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
    }
}