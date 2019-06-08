namespace Raft.Entities
{
    public class LogEntry
    {
        public OperationType Type { get; set; }
        public string Value { get; set; }
    }

    public enum OperationType
    {
        Add,
        Update,
        Remove
    }
}