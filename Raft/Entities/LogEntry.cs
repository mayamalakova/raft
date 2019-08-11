using System;

namespace Raft.Entities
{
    public class LogEntry
        
    {
        public OperationType Type { get; set; }
        public string Value { get; }
        public Guid Id { get; }

        public LogEntry(OperationType type, string value, Guid id)
        {
            Type = type;
            Value = value;
            Id = id;
        }

    }

    public enum OperationType
    {
        Add,
        Update,
        Remove,
        Commit
    }
}