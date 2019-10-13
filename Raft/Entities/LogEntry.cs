using System;

namespace Raft.Entities
{
    public class LogEntry
        
    {
        public OperationType Type { get; set; }
        public string Value { get; }
        public Guid Id { get; }

        public int Term { get; }

        public LogEntry(OperationType type, string value, Guid id, int term)
        {
            Type = type;
            Value = value;
            Id = id;
            Term = term;
        }

    }

    public enum OperationType
    {
        Update,
        Commit
    }
}