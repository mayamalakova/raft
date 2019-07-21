using System;
using Raft.Entities;

namespace Raft.Election
{
    public class FollowerMessageResponseStrategy: IMessageResponseStrategy
    {
        private Node Node { get; }

        public FollowerMessageResponseStrategy(Node node)
        {
            Node = node;
        }
        
        public void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    Node.UpdateLog(message, message.Id);
                    Node.ConfirmLogUpdate(message.Id);
                    break;

                case MessageType.LogUpdateConfirmation:
                    break;

                case MessageType.LogCommit:
                    Node.CommitLog(message);
                    break;

                case MessageType.ValueUpdate:
                    break;
                case MessageType.Info:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}