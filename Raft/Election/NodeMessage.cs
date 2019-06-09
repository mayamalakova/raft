using System;

namespace Raft.Election
{
    public class NodeMessage
    {
        public string Value { get; set; }
        public MessageType Type { get; set; }

        public string SenderName { get; set; }

        public Guid Id { get; set; }
        
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
        LogUpdateReceived,
        LogCommit,
        Info,
    }
}