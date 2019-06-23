using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Raft.Communication;
using Raft.Election;
using Raft.Time;

namespace Raft.Runner
{
    public class RaftRunner
    {
        private bool _keepRunning = true;
        private static readonly TimeoutGenerator TimeoutGenerator = new TimeoutGenerator();
        private readonly IMessageBroker _messageBroker = new MessageBroker();

        public void Run()
        {
            ConfigureLogging();

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
                    _messageBroker.Broadcast(newValue);
                }
            }
        }

        private void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/raft.log",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForAllLevels(fileTarget);

            LogManager.Configuration = config;
        }

        private void StartLeaderNode(string name)
        {
            var node = new LeaderNodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            node.Start();
        }

        private void StartNode(string name)
        {
            var node = new NodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            node.Start();
        }
    }
}