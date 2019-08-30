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
                    BecomeFollowerIfSentFromNewerTerm(message);
                    break;
                case MessageType.LogCommit:
                    BecomeFollowerIfSentFromNewerTerm(message);
                    break;
                case MessageType.Info:
                    BecomeFollowerIfSentFromNewerTerm(message);
                    break;
                case MessageType.VoteRequest:
                    if (FromHigherTerm(message))
                    {
                        BecomeFollower(message);
                        Node.Vote(message.Term, message.SenderName, message.Id);
                    }

                    break;
                case MessageType.LeaderVote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BecomeFollowerIfSentFromNewerTerm(NodeMessage message)
        {
            if (FromHigherTerm(message))
            {
                BecomeFollower(message);
            }
        }

        private bool FromHigherTerm(NodeMessage message)
        {
            return message.Term >= Node.Status.Term;
        }

        private void ResetUpdateConfirmations()
        {
            _status.ConfirmedNodes.Clear();
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