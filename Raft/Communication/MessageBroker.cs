using System.Collections.ObjectModel;
using Raft.Election;
using Raft.Entities;

namespace Raft.Communication
{
    public class MessageBroker
    {
        private readonly Collection<NodeRunner> _listeners;

        public MessageBroker(Collection<NodeRunner> listeners)
        {
            _listeners = listeners;
        }

        public void Broadcast(string message)
        {
            foreach (var listener in _listeners)
            {
                listener.ReceiveMessage(new NodeMessage(message, true));
            }
        }
        
        public void Send(string message)
        {
            foreach (var listener in _listeners)
            {
                listener.ReceiveMessage(new NodeMessage(message, false));
            }
        }
    }
}