using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner: IMessageBrokerListener
    {
        private readonly Timer _timer;
        protected readonly IMessageBroker Broker;
        protected Node Node { get; }
        protected NodeStatus Status { get; set; }

        private readonly Collection<LogEntry> _log = new Collection<LogEntry>();

        public NodeRunner(string name, int electionTimeout, IMessageBroker broker)
        {
            Broker = broker;
            Broker.Register(this);
            _timer = new Timer(electionTimeout * 10);

            Node = new Node(name);
            Status = NodeStatus.Follower;
        }
        
        public virtual void ReceiveMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    UpdateLog(message, message.Id);
                    ConfirmLogUpdate(message.Id);
                    break;

                case MessageType.LogUpdateReceived:
                    break;
                
                case MessageType.LogCommit:
                    CommitLog(message);
                    break;
                
                case MessageType.ValueUpdate:
                    break;
                case MessageType.Info:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ConfirmLogUpdate(Guid entryId)
        {
            Console.WriteLine($"node {Node.Name} confirms {entryId}");
            var nodeMessage = new NodeMessage(null, MessageType.LogUpdateReceived, Node.Name, entryId);
            Broker.Broadcast(nodeMessage);
        }

        protected void UpdateLog(NodeMessage message, Guid entryId)
        {
            var logEntry = new LogEntry(OperationType.Update, message.Value, entryId);
            _log.Add(logEntry);
        }
        
        protected void CommitLog(NodeMessage message)
        {
            var logEntry = LastLogEntry();
            if (logEntry?.Id != message.Id || logEntry.Type == OperationType.Commit)
            {
                return;
            }
            Console.WriteLine($"{Node.Name} committing {message.Id}");
            logEntry.Type = OperationType.Commit;
            Node.Value = message.Value;
        }

        public void Start()
        {
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
//                Console.WriteLine($"node {Node.Name} - timer elapsed");
            };
        }

        protected void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
        protected LogEntry LastLogEntry()
        {
            return _log.Count > 0 ? _log.Last() : null;
        }
        
    }
}