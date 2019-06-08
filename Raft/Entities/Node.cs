using System;
using System.Timers;

namespace Raft.Entities
{
    public class Node
    {
        private Timer _timer;
        private string Value { get; }
        private NodeStatus Status { get; }
        public int ElectionTimeout { get; }
        private string Name { get; set; }

        public Node(string name, NodeStatus status, int electionTimeout, string value)
        {
            Value = value;
            Name = name;
            Status = status;
            ElectionTimeout = electionTimeout * 10;
        }

        public void ReceiveMessage(string message)
        {
            Console.WriteLine($"node {Name} got message {message}");
            RestartElectionTimeout();
        }

        private void RestartElectionTimeout()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void Start()
        {
            _timer = new Timer(ElectionTimeout);
            Console.WriteLine($"node {Name} - timer started with timeout {ElectionTimeout}");
            _timer.Start();
            _timer.AutoReset = true;
            
            _timer.Elapsed += (sender, args) =>
            {
                Console.WriteLine($"node {Name} - timer elapsed");
            };
        }
    }
}