namespace Raft.Election
{
    public interface IMessageBrokerListener
    {
        void ReceiveMessage(NodeMessage message);
    }
}