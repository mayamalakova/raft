using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using Raft.Communication;
using Raft.Election;
using Raft.Entities;
using Raft.Time;

namespace Raft.Runner
{
    public class RaftRunner
    {
        private static readonly TimeoutGenerator TimeoutGenerator = new TimeoutGenerator();
        private readonly Collection<IMessageBrokerListener> _nodeRunners = new Collection<IMessageBrokerListener>();
        public IMessageBroker Broker { get; } = new MessageBroker();

        public void Run()
        {
            ConfigureLogging();

            StartLeaderNode("L");
            StartNode("A");
            StartNode("B");
            StartNode("C");

            while (true)
            {
                var command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                {
                    return;
                }

                if (command.Equals("?"))
                {
                    ShowHelp();
                }
                else if (command.StartsWith("value"))
                {
                    UpdateValue(command);
                    DisplayStatus();
                }
                else if (command.StartsWith("status"))
                {
                    DisplayStatus();
                }
                else if (command.StartsWith("disconnect"))
                {
                    DisconnectNode(command);
                }
                else if (command.StartsWith("connect"))
                {
                    ConnectNode(command);
                }
                else
                {
                    Console.WriteLine("Write ? and press Enter to see the options, or press Enter to exit");
                }
            }
        }

        private void DisplayStatus()
        {
            foreach (var nodeRunner in _nodeRunners)
            {
                nodeRunner.DisplayStatus();
            }
        }

        private void ConnectNode(string command)
        {
            var entries = command.Split(' ');
            Broker.Connect(entries[1]);
        }

        private void DisconnectNode(string command)
        {
            var entries = command.Split(' ');
            Broker.Disconnect(entries[1]);
        }

        private void UpdateValue(string command)
        {
            var entries = command.Split(' ');
            var value = entries[1];
            Broker.Broadcast(value);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("You can use the following commands: ");
            Console.WriteLine("status - to see the current node values ");
            Console.WriteLine("value - to enter new value ");
            Console.WriteLine("disconnect - to disconnect a node");
            Console.WriteLine("connect - to reconnect a node");
        }

        private void ConfigureLogging()
        {
            var config = new LoggingConfiguration();

            var fileTarget = CreateFileTarget();
            var consoleTarget = CreateConsoleTarget();
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            config.AddRuleForOneLevel(LogLevel.Debug, fileTarget);
            config.AddRuleForOneLevel(LogLevel.Info, fileTarget);
            config.AddRuleForOneLevel(LogLevel.Warn, fileTarget);
            config.AddRuleForOneLevel(LogLevel.Error, fileTarget);
            config.AddRuleForOneLevel(LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        private ColoredConsoleTarget CreateConsoleTarget()
        {
            return new ColoredConsoleTarget("target1")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
            };
        }

        private static FileTarget CreateFileTarget()
        {
            return new FileTarget("target2")
            {
                FileName = "${basedir}/raft.log",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
        }

        private void StartLeaderNode(string name)
        {
            var nodeRunner = InitializeLeader(name);
            _nodeRunners.Add(nodeRunner);
            nodeRunner.Start();
        }

        private void StartNode(string name)
        {
            var nodeRunner = InitializeFollower(name);
            _nodeRunners.Add(nodeRunner);
            nodeRunner.Start();
        }

        public LeaderNodeRunner InitializeLeader(string name)
        {
            var node = new Node(name, Broker);
            var nodeRunner = new LeaderNodeRunner(node, TimeoutGenerator.GenerateElectionTimeout());
            Broker.Register(nodeRunner);
            return nodeRunner;
        }

        public NodeRunner InitializeFollower(string name)
        {
            var node = new Node(name, Broker);
            var nodeRunner = new NodeRunner(node, TimeoutGenerator.GenerateElectionTimeout());
            Broker.Register(nodeRunner);
            return nodeRunner;
        }
    }
}