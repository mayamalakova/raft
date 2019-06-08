using System.Collections.ObjectModel;
using Raft.Election;
using Raft.Entities;

namespace Raft.Communication
{
    public class MessageBroadcaster
    {
        private readonly Collection<NodeRunner> _listeners;

        public MessageBroadcaster(Collection<NodeRunner> listeners)
        {
            _listeners = listeners;
        }

        public void Broadcast(NodeMessage message)
        {
            foreach (var listener in _listeners)
            {
                listener.ReceiveMessage(message);
            }
        }
    }
}