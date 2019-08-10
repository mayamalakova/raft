using Raft.Entities;

namespace Raft.Election
{
    public interface IMessageBrokerListener
    {
        void ReceiveMessage(NodeMessage message);
        string Name { get; }
    }
}