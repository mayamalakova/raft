using System;
using Raft.Election;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    public class LeaderMessageResponseStrategy: IMessageResponseStrategy
    {
        private readonly LeaderStatus _status;

        private readonly int _nodesCount;
        private Node Node { get; }
        

        public LeaderMessageResponseStrategy(Node node, int nodesCount)
        {
            _nodesCount = nodesCount;
            Node = node;
            _status = node.Status as LeaderStatus;
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

                    _status.ConfirmedNodes.Add(message.SenderName);
                    if (_status.ConfirmedNodes.Count > _nodesCount / 2)
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