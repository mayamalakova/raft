namespace Raft.Election
{
    public interface IMessageBroker
    {
        void Broadcast(NodeMessage message);
        void Broadcast(string newValue);
        
        void Register(IMessageBrokerListener listener);
        
        void Disconnect(string node);
        void Connect(string node);
    }
}