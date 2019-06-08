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
        private readonly MessageBroker _messageBroker = new MessageBroker();

        public void Run()
        {
            StartLeaderNode("L");
            StartNode("A");
            StartNode("B");
            StartNode("C");
           
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
                    _messageBroker.Send(newValue);
                }
            }
        }

        private NodeRunner StartLeaderNode(string name)
        {
            var node = new LeaderNodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            node.Start();
            return node;
        }

        private NodeRunner StartNode(string name)
        {
            var node = new NodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            node.Start();
            return node;
        }
    }
}