using System;
using System.Timers;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner
    {
        private Node Node { get; }
        protected NodeStatus Status { get; set; }

        private readonly Timer _timer;

        public NodeRunner(string name, int electionTimeout)
        {

            Node = new Node(name);
            Status = NodeStatus.Follower;
            Console.WriteLine($"node {Node.Name} - has timeout {electionTimeout}");
            _timer = new Timer(electionTimeout * 10);
        }
        
        public void ReceiveMessage(NodeMessage message)
        {
            if (message.IsBroadcast)
            {
                Console.WriteLine($"node {Node.Name} got message {message.Message}");
                RestartElectionTimeout();
            }
        }

        public void Start()
        {
            Console.WriteLine($"node {Node.Name} - timer started");
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Console.WriteLine($"node {Node.Name} - timer elapsed");
            };
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }
        
    }
}