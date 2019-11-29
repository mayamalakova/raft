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

        void Send(NodeMessage nodeMessage, string recipient);
        
        void Register(IMessageBrokerListener listener);
        
        void Disconnect(IEnumerable<string> nodes);
        void Connect(IEnumerable<string> nodes);

        IEnumerable<IMessageBrokerListener> Listeners { get; }
    }
}