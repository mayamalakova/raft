using System;
using System.Collections.ObjectModel;
using Raft.Communication;
using Raft.Entities;

namespace Raft
{
    public class RaftRunner
    {
        private bool _keepRunning = true;

        public void Run()
        {
            var nodes = new Collection<Node>()
            {
                new Node("A", NodeStatus.Follower, null),
                new Node("B", NodeStatus.Follower, null),
                new Node("C", NodeStatus.Follower, null)
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
    }
}