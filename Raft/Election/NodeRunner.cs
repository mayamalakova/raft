using System;
using System.Timers;
using Raft.Entities;

namespace Raft.Election
{
    public class NodeRunner
    {
        private Node Node { get; }
        private NodeStatus Status { get; }
        
        private readonly Timer _timer;

        public NodeRunner(string name, int electionTimeout)
        {

            Node = new Node(name);
            Status = NodeStatus.Follower;
            Console.WriteLine($"node {Node.Name} - has timeout {electionTimeout}");
            _timer = new Timer(electionTimeout * 10);
        }
        
        public void ReceiveMessage(string message)
        {
            Console.WriteLine($"node {Node.Name} got message {message}");
            RestartElectionTimeout();
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