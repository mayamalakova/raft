using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Raft.Entities;

namespace Raft.Communication
{

    public class MessageBroker : IMessageBroker
    {
        private readonly ICollection<IMessageBrokerListener> _listeners = new List<IMessageBrokerListener>();
        private readonly Collection<string> _disconnectedNodes = new Collection<string>();
        
        public IEnumerable<IMessageBrokerListener> Listeners => _listeners;

        public void Broadcast(string newValue)
        {
            var nodeMessage = new NodeMessage(-1, newValue, MessageType.ValueUpdate, null, Guid.Empty);
            NotifyListeners(nodeMessage);
        }

        public void Send(NodeMessage nodeMessage, string recipientName)
        {
            if (_disconnectedNodes.Contains(nodeMessage.SenderName) ||
                _disconnectedNodes.Contains(recipientName))
            {
                return;
            }
            var recipient = _listeners.First(x => x.Name == recipientName);
            recipient?.ReceiveMessage(nodeMessage);
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

        public void Disconnect(IEnumerable<string> nodes)
        {
            foreach (var node in nodes)
            {
                _disconnectedNodes.Add(node);
            }
        }

        public void Connect(IEnumerable<string> nodes)
        {
            foreach (var node in nodes)
            {
                _disconnectedNodes.Remove(node);
            }
        }
        
        public bool IsConnected(IMessageBrokerListener nodeRunner)
        {
            return !_disconnectedNodes.Contains(nodeRunner.Name);
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