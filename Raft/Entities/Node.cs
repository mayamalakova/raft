using System;

namespace Raft.Entities
{
    public class Node
    {
        private string Value { get; }
        private NodeStatus Status { get; }
        private string Name { get; set; }

        public Node(string name, NodeStatus status, string value)
        {
            Value = value;
            Name = name;
            Status = status;
        }

        public void ReceiveMessage(string message)
        {
            Console.WriteLine($"node {Name} got message {message}");
        }
    }
}