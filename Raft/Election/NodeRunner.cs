using System;
using System.Collections.ObjectModel;
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
            if (message.Type == MessageType.LogUpdate)
            {
                Console.WriteLine($"node {Node.Name} got LogUpdate {message.Value}");
                UpdateLog(message);
                ConfirmLogUpdate();
            }
        }

        private void ConfirmLogUpdate()
        {
            var nodeMessage = new NodeMessage(null, MessageType.LogUpdateReceived, Node.Name);
            Broker.Broadcast(nodeMessage);
        }

        private void UpdateLog(NodeMessage message)
        {
            var logEntry = new LogEntry(OperationType.Update, message.Value);
            _log.Add(logEntry);
        }

        public void Start()
        {
            Console.WriteLine($"node {Node.Name} - timer started");
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
        
    }
}