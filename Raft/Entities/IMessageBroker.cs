using System.Collections.Generic;
using Raft.Communication;
using Raft.Election;

namespace Raft.Entities
{
    public interface IMessageBroker
    {
        void Broadcast(NodeMessage message);
        void Broadcast(string newValue, int term);
        
        void Register(IMessageBrokerListener listener);
        
        void Disconnect(string node);
        void Connect(string node);

        IEnumerable<IMessageBrokerListener> Listeners { get; }
    }
}