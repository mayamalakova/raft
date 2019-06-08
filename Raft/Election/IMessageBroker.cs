namespace Raft.Election
{
    public interface IMessageBroker
    {
        void Broadcast(string message);
        void Send(string message);
        void Register(NodeRunner nodeRunner);
    }
}