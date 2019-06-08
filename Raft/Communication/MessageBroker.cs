using System.Collections.Generic;
using Raft.Election;

namespace Raft.Communication
{
    public class MessageBroker: IMessageBroker
    {
        private readonly List<IMessageBrokerListener> _listeners = new List<IMessageBrokerListener>();

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