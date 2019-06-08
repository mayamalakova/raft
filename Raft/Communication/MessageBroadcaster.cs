using System.Collections.ObjectModel;
using Raft.Entities;

namespace Raft.Communication
{
    public class MessageBroadcaster
    {
        private readonly Collection<Node> _listeners;

        public MessageBroadcaster(Collection<Node> listeners)
        {
            _listeners = listeners;
        }

        public void Broadcast(string message)
        {
            foreach (var listener in _listeners)
            {
                listener.ReceiveMessage(message);
            }
        }
    }
}