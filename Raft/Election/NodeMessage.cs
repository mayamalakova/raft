namespace Raft.Election
{
    public class NodeMessage
    {
        public string Value { get; set; }
        public MessageType Type { get; set; }

        public string SenderName { get; set; }
        
        public NodeMessage(string value, MessageType type, string senderName)
        {
            Value = value;
            Type = type;
            SenderName = senderName;
        }
    }

    public enum MessageType
    {
        ValueUpdate,
        LogUpdate,
        LogCommit,
        Info
    }
}