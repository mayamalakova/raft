namespace Raft.Election
{
    public interface IMessageBroker
    {
        void Broadcast(NodeMessage message);
        
        void Send(string message);
        void Send(NodeMessage message);

        
        void Register(IMessageBrokerListener listener);
    }
}