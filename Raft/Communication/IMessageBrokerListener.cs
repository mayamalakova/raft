using Raft.Entities;

namespace Raft.Communication
{
    /// <summary>
    /// An entity interested in receiving Raft messages
    /// </summary>
    public interface IMessageBrokerListener
    {
        void ReceiveMessage(NodeMessage message);
        
        string Name { get; }
        void Timeout();
    }
}