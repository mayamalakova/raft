using System;

namespace Raft.Entities
{
    public class NodeMessage
    {
        public string Value { get; private set; }
        public MessageType Type { get; private set; }

        public string SenderName { get; private set; }

        public Guid Id { get; private set; }
        
        public NodeMessage(string value, MessageType type, string senderName, Guid id)
        {
            Value = value;
            Type = type;
            SenderName = senderName;
            Id = id;
        }
    }

    public enum MessageType
    {
        ValueUpdate,
        LogUpdate,
        LogUpdateConfirmation,
        LogCommit,
        Info,
    }
}