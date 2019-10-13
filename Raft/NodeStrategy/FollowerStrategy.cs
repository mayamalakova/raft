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
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    if (message.Term == Node.Status.Term)
                    {
                        ConfirmLogUpdate(message);
                    }

                    break;

                case MessageType.LogUpdateConfirmation:
                    break;

                case MessageType.LogCommit:
                    if (message.Term == Node.Status.Term)
                    {
                        CommitLog(message);
                    }

                    break;

                case MessageType.ValueUpdate:
                    break;

                case MessageType.Info:
                    break;

                case MessageType.VoteRequest:
                    if (!Node.HasVotedInTerm(message.Term) && CandidateIsUpToDate(message))
                    {
                        Node.Status.Term = message.Term;
                        Node.Status.LastVote = message.Term;
                        Vote(message);
                    }

                    break;
                case MessageType.LeaderVote:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool CandidateIsUpToDate(NodeMessage message)
        {
            if (Node.LastLogEntry() == null)
            {
                
                return true;
            }
            
            var (term, index) = ParseLastLogEntryInfo(message);
            if (term < Node.LastLogEntry().Term || term == Node.LastLogEntry().Term && index < Node.Log.Count - 1)
            {
                Logger.Debug($"{Node.Name} denyies vote {message.Value} to {message.SenderName}");
                return false;
            }

            return true;
        }

        private static (int term, int index) ParseLastLogEntryInfo(NodeMessage message)
        {
            var lastLogEntryInfo = message.Value.Split(",");
            return (int.Parse(lastLogEntryInfo[0]), int.Parse(lastLogEntryInfo[1]));
        }

        private void Vote(NodeMessage message)
        {
            Node.Vote(message.Term, message.SenderName, message.Id);
        }
    }
}