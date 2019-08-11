using System.Collections.Generic;
using Raft.Communication;

namespace Raft.Entities
{
    /// <summary>
    /// Broadcasts messages to subscribed listeners
    /// </summary>
    public interface IMessageBroker
    {
        void Broadcast(NodeMessage message);
        void Broadcast(string newValue);
        
        void Register(IMessageBrokerListener listener);
        
        void Disconnect(string node);
        void Connect(string node);

        IEnumerable<IMessageBrokerListener> Listeners { get; }
    }
}