namespace Raft.Entities
{
    public class LogEntry
        
    {
        public OperationType Type { get; set; }
        public string Value { get; set; }
        
        public LogEntry(OperationType type, string value)
        {
            Type = type;
            Value = value;
        }

    }

    public enum OperationType
    {
        Add,
        Update,
        Remove
    }
}