using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using Raft.Election;

namespace Raft.Entities
{
    public class Node
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private string _value;
        
        private readonly ICollection<INodeSubscriber> _subscribers = new List<INodeSubscriber>();
        
        public string Name { get; }
        
        public Collection<LogEntry> Log { get; } = new Collection<LogEntry>();

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

        public Node(string name, string value)
        {
            Value = value;
            Name = name;
        }

        public Node(string name)
        {
            Name = name;
        }

        public void UpdateLog(NodeMessage message, Guid entryId)
        {
            var logEntry = new LogEntry(OperationType.Update, message.Value, entryId);
            Log.Add(logEntry);
        }

        public void CommitLog(NodeMessage message)
        {
            var logEntry = LastLogEntry();
            if (logEntry?.Id != message.Id || logEntry.Type == OperationType.Commit)
            {
                return;
            }
            Logger.Debug($"{Name} committing {message.Id}");
            
            logEntry.Type = OperationType.Commit;
            Value = logEntry.Value;
        }

        protected internal LogEntry LastLogEntry()
        {
            return Log.Count > 0 ? Log.Last() : null;
        }

        public void Subscribe(INodeSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }
    }
}