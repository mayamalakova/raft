using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Raft.Election;

namespace Raft.Communication
{
    public class MessageBroker : IMessageBroker
    {
        private readonly ConcurrentBag<IMessageBrokerListener> _listeners = new ConcurrentBag<IMessageBrokerListener>();
        private readonly Collection<string> _disconnectedNodes = new Collection<string>();

        public void Broadcast(string newValue)
        {
            var nodeMessage = new NodeMessage(newValue, MessageType.ValueUpdate, null, Guid.Empty);
            NotifyListeners(nodeMessage);
        }

        public void Broadcast(NodeMessage nodeMessage)
        {
            if (!_disconnectedNodes.Contains(nodeMessage.SenderName))
            {
                NotifyListeners(nodeMessage);
            }
        }

        public void Register(IMessageBrokerListener listener)
        {
            _listeners.Add(listener);
        }

        public void Disconnect(string node)
        {
            _disconnectedNodes.Add(node);
        }

        public void Connect(string node)
        {
            _disconnectedNodes.Remove(node);
        }

        private void NotifyListeners(NodeMessage nodeMessage)
        {
            foreach (var listener in _listeners)
            {
                if (!_disconnectedNodes.Contains(listener.Name))
                {
                    listener.ReceiveMessage(nodeMessage);
                }
            }
        }
    }
}