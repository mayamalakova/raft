using System;
using System.Collections.ObjectModel;
using Raft.Communication;
using Raft.Entities;

namespace Raft
{
    public class RaftRunner
    {
        private bool _keepRunning = true;
        private static readonly TimeoutGenerator TimeoutGenerator = new TimeoutGenerator();

        public void Run()
        {
            var node1 = StartNode("A");
            var node2 = StartNode("B");
            var node3 = StartNode("C");

            var nodes = new Collection<Node>()
            {
                node1,
                node2,
                node3,
            };
            var messageBroadcaster = new MessageBroadcaster(nodes);
            messageBroadcaster.Broadcast("start");

            while (_keepRunning)
            {
                Console.WriteLine("Client message:");
                var newValue = Console.ReadLine();

                if (string.IsNullOrEmpty(newValue))
                {
                    _keepRunning = false;
                }
                messageBroadcaster.Broadcast(newValue);
            }
        }

        private static Node StartNode(string name)
        {
            var node = new Node(name, NodeStatus.Follower, TimeoutGenerator.GenerateElectionTimeout(), null);
            node.Start();
            return node;
        }
    }
}