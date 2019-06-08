using System;
using System.Collections.ObjectModel;
using Raft.Communication;
using Raft.Election;
using Raft.Time;

namespace Raft.Runner
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

            var nodes = new Collection<NodeRunner>()
            {
                node1,
                node2,
                node3,
            };
            var messageBroadcaster = new MessageBroadcaster(nodes);
            while (_keepRunning)
            {
                Console.WriteLine("Client message:");
                var newValue = Console.ReadLine();

                if (string.IsNullOrEmpty(newValue))
                {
                    _keepRunning = false;
                }
                else
                {
                    messageBroadcaster.Broadcast(newValue);
                }
            }
        }

        private static NodeRunner StartNode(string name)
        {
            var node = new NodeRunner(name, TimeoutGenerator.GenerateElectionTimeout());
            node.Start();
            return node;
        }
    }
}