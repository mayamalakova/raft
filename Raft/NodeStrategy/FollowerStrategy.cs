using System;
using NLog;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Follower node strategy for responding to Raft messages
    /// </summary>
    public class FollowerStrategy : BaseStrategy, IMessageResponseStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FollowerStrategy(Node node)
        {
            Node = node;
        }

        public void RespondToMessage(NodeMessage message)
        {
            if (IsFromOlderTerm(message))
            {
                return;
            }

            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    Node.Status.Term = message.Term;
                    ConfirmLogUpdate(message);
                    break;

                case MessageType.LogUpdateConfirmation:
                    break;

                case MessageType.LogCommit:
                    Node.Status.Term = message.Term;
                    CommitLog(message);
                    break;

                case MessageType.ValueUpdate:
                    break;

                case MessageType.Info:
                    Node.Status.Term = message.Term;
                    break;

                case MessageType.VoteRequest:
                    if (Node.HasVotedInTerm(message.Term))
                    {
                        break;
                    }

                    if (!CandidateIsUpToDate(message))
                    {
                        Logger.Debug($"{Node.Name} denies vote {message.Value} to {message.SenderName}");
                        break;
                    }

                    Node.Status.Term = message.Term;
                    Node.Status.LastVote = message.Term;
                    Vote(message);

                    break;
                case MessageType.LeaderVote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Vote(NodeMessage message)
        {
            Node.Vote(message.Term, message.SenderName, message.Id);
        }
    }
}