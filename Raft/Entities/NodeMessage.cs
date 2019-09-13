using System;

namespace Raft.Entities
{
    public class NodeMessage
    {
        public string Value { get; }
        public MessageType Type { get; }

        public string SenderName { get; }

        public int Term { get; }

        public Guid Id { get; }
        
        public NodeMessage(int term, string value, MessageType type, string senderName, Guid id)
        {
            Value = value;
            Type = type;
            SenderName = senderName;
            Id = id;
            Term = term;
        }
    }

    public enum MessageType
    {
        ValueUpdate,
        LogUpdate,
        LogUpdateConfirmation,
        LogCommit,
        Info,
        VoteRequest,
        LeaderVote
    }
}