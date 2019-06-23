using System.Collections.Generic;

namespace Raft.Entities
{
    public class Node
    {
        private string _value;
        
        private readonly ICollection<INodeSubscriber> _subscribers = new List<INodeSubscriber>();

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                foreach (var subscriber in _subscribers)
                {
                    subscriber.NodeValueChanged(Name, value);
                }
            }
        }

        public string Name { get; }
        
        private IEnumerable<LogEntry> Log { get; }

        public Node(string name, string value)
        {
            Value = value;
            Name = name;
            Log = new List<LogEntry>();
        }

        public Node(string name)
        {
            Name = name;
        }

        public void Subscribe(INodeSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }
    }
}