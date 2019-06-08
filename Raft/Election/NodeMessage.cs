namespace Raft.Election
{
    public class NodeMessage
    {
        public string Message { get; set; }
        public bool IsBroadcast { get; set; }
        
        public NodeMessage(string message, bool isBroadcast)
        {
            Message = message;
            IsBroadcast = isBroadcast;
        }


    }
}