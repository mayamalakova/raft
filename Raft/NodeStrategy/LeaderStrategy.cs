using System;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Leader node strategy for responding to Raft messages
    /// </summary>
    public class LeaderStrategy : BaseStrategy, IMessageResponseStrategy
    {
        private readonly LeaderStatus _status;

        private readonly int _nodesCount;

        public LeaderStrategy(Node node, int nodesCount)
        {
            _nodesCount = nodesCount;
            Node = node;
            _status = node.Status as LeaderStatus;
        }

        public void RespondToMessage(NodeMessage message)
        {
            if (message.Term > Node.Status.Term)
            {
                BecomeFollower(message);
                new FollowerStrategy(Node).RespondToMessage(message);
                return;
            }

            if (message.Type != MessageType.ValueUpdate && message.Term < _status.Term)
            {
                return;
            }
            
            switch (message.Type)
            {
                case MessageType.ValueUpdate:
                    ResetUpdateConfirmations();
                    RequestLogUpdate(message);
                    break;

                case MessageType.LogUpdateConfirmation:
                    if (ConfirmsLastEntry(message))
                    {
                        AddConfirmation(message);
                        if (HasMajority())
                        {
                            CommitLogEntry(message);
                        }
                    }

                    break;

                case MessageType.LogUpdate:
                    break;
                case MessageType.LogCommit:
                    break;
                case MessageType.Info:
                    break;
                case MessageType.VoteRequest:
                    break;
                case MessageType.LeaderVote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetUpdateConfirmations()
        {
            _status.ConfirmedNodes.Clear();
            _status.ConfirmedNodes.Add(Node.Name);
        }

        private void RequestLogUpdate(NodeMessage message)
        {
            var entryId = Guid.NewGuid();
            Node.UpdateLog(message, entryId);
            Node.SendLogUpdateRequest(message, entryId);
        }

        private void CommitLogEntry(NodeMessage message)
        {
            Node.CommitLog(message);
            Node.SendCommit(message);
        }

        private bool HasMajority()
        {
            return _status.ConfirmedNodes.Count > _nodesCount / 2;
        }

        private void AddConfirmation(NodeMessage message)
        {
            _status.ConfirmedNodes.Add(message.SenderName);
        }

        private bool ConfirmsLastEntry(NodeMessage message)
        {
            return message.Id == Node.LastLogEntry()?.Id;
        }
    }
}