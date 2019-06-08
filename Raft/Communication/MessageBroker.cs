using System.Collections.Generic;
using Raft.Election;

namespace Raft.Communication
{
    public class MessageBroker: IMessageBroker
    {
        private readonly List<NodeRunner> _listeners = new List<NodeRunner>();

        public void Broadcast(string message)
        {
            var nodeMessage = new NodeMessage(message, true);
            NotifyListeners(nodeMessage);
        }

        public void Broadcast(NodeMessage nodeMessage)
        {
            NotifyListeners(nodeMessage);
        }

        public void Send(string message)
        {
            var nodeMessage = new NodeMessage(message, false);
            NotifyListeners(nodeMessage);
        }

        public void Send(NodeMessage nodeMessage)
        {
            NotifyListeners(nodeMessage);
        }

        public void Register(NodeRunner node)
        {
            _listeners.Add(node);
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