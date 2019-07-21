using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using NLog;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner: IMessageBrokerListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Timer _timer;
        protected readonly IMessageBroker Broker;
        protected Node Node { get; }
        protected NodeStatus Status { get; set; }
        public string Name => Node.Name;

        public NodeRunner(Node node, int electionTimeout, IMessageBroker broker)
        {
            Node = node;
            Status = NodeStatus.Follower;
            
            Broker = broker;
            Broker.Register(this);
            _timer = new Timer(electionTimeout * 10);
        }
        
        public void ReceiveMessage(NodeMessage message)
        {
            if (Node.Name.Equals(message.SenderName))
            {
                return;
            }

            RestartElectionTimeout();
            
            RespondToMessage(message);
        }

        public virtual void DisplayStatus()
        {
            Console.WriteLine($"{Name} - {Node.Value}");
        }

        protected virtual void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    Node.UpdateLog(message, message.Id);
                    ConfirmLogUpdate(message.Id);
                    break;

                case MessageType.LogUpdateConfirmation:
                    break;

                case MessageType.LogCommit:
                    Node.CommitLog(message);
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
            Logger.Debug($"node {Node.Name} confirms {entryId}");
            
            var nodeMessage = new NodeMessage(null, MessageType.LogUpdateConfirmation, Node.Name, entryId);
            Broker.Broadcast(nodeMessage);
        }

        public void Start()
        {
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Logger.Trace($"node {Node.Name} - timer elapsed");
            };
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
        
        public void Subscribe(INodeSubscriber subscriber)
        {
            Node.Subscribe(subscriber);
        }
    }
}