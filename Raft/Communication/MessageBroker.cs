using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Raft.Election;
using Raft.Entities;

namespace Raft.Communication
{
    public class MessageBroker: IMessageBroker
    {
        private readonly List<NodeRunner> _listeners = new List<NodeRunner>();

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

        public void Register(NodeRunner node)
        {
            _listeners.Add(node);
        }
    }
}