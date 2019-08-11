using Raft.Entities;

namespace Raft.Communication
{
    public interface IMessageBrokerListener
    {
        void ReceiveMessage(NodeMessage message);
        
        string Name { get; }
        bool IsLeading { get; }
        int Term { get; }
    }
}