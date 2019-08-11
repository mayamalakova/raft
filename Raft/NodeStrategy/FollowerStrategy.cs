using System;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Follower node strategy for responding to Raft messages
    /// </summary>
    public class FollowerStrategy: BaseStrategy, IMessageResponseStrategy
    {

        public FollowerStrategy(Node node)
        {
            Node = node;
        }
        
        public void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    ConfirmLogUpdate(message);
                    break;

                case MessageType.LogUpdateConfirmation:
                    break;

                case MessageType.LogCommit:
                    CommitLog(message);
                    break;

                case MessageType.ValueUpdate:
                    break;
                
                case MessageType.Info:
                    break;
                
                case MessageType.VoteRequest:
                    if (!Node.HasVotedInTerm(message.Term))
                    {
                        Vote(message);
                    }
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

        private void CommitLog(NodeMessage message)
        {
            Node.CommitLog(message);
        }
    }
}