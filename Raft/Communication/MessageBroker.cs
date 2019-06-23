using System;
using System.Collections.Concurrent;
using Raft.Election;

namespace Raft.Communication
{
    public class MessageBroker: IMessageBroker
    {
        private readonly ConcurrentBag<IMessageBrokerListener> _listeners = new ConcurrentBag<IMessageBrokerListener>();

        public void Broadcast(string newValue)
        {
            var nodeMessage = new NodeMessage(newValue, MessageType.ValueUpdate, null, Guid.Empty);
            NotifyListeners(nodeMessage);
        }

        public void Broadcast(NodeMessage nodeMessage)
        {
            NotifyListeners(nodeMessage);
        }

        public void Register(IMessageBrokerListener listener)
        {
            _listeners.Add(listener);
        }
        
        private void NotifyListeners(NodeMessage nodeMessage)
        {
            foreach (var listener in _listeners)
            {
                listener.ReceiveMessage(nodeMessage);
            }
        }
    }
}