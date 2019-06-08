namespace Raft.Election
{
    public class NodeMessage
    {
        public string Message { get; set; }
        public bool IsBroadcast { get; set; }
        public MessageType Type { get; set; }

        public string SenderName { get; set; }
        
        public NodeMessage(string message, bool isBroadcast)
        {
            Message = message;
            IsBroadcast = isBroadcast;
            Type = MessageType.Info;
        }
        
        public NodeMessage(string message, bool isBroadcast, MessageType type, string senderName)
        {
            Message = message;
            IsBroadcast = isBroadcast;
            Type = type;
            SenderName = senderName;
        }
    }

    public enum MessageType
    {
        LogUpdate,
        LogCommit,
        Info
    }
}