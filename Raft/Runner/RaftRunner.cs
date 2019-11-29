using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using Raft.Communication;
using Raft.Entities;
using Raft.NodeStrategy;
using Raft.Time;

namespace Raft.Runner
{
    public class RaftRunner
    {
        private static readonly string[] Nodes = {"L", "A", "B", "C", "D"};
        
        private static readonly TimeoutGenerator TimeoutGenerator = new TimeoutGenerator();
        private readonly Collection<IMessageBrokerListener> _nodeRunners = new Collection<IMessageBrokerListener>();
        public IMessageBroker Broker { get; } = new MessageBroker();

        public void Run()
        {
            ConfigureLogging();

            StartLeaderNode(Nodes[0]);
            foreach (var node in Nodes.TakeLast(Nodes.Length - 1))
            {
                StartNode(node);
            }

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
                    DisconnectNodes(command);
                }
                else if (command.StartsWith("connect"))
                {
                    ConnectNodes(command);
                }
                else if (command.StartsWith("timeout"))
                {
                    TimeoutNodes(command);
                }
                else
                {
                    Console.WriteLine("Write ? and press Enter to see the options, or press Enter to exit");
                }
            }
        }

        private void TimeoutNodes(string command)
        {
            var nodes = GetNodesForCommand(command);
            foreach (var node in nodes)
            {
                var runner = _nodeRunners.First(x => x.Name == node);
                runner.Timeout();
            }
        }

        private void DisplayStatus()
        {
            foreach (var nodeRunner in _nodeRunners)
            {
                Console.WriteLine(nodeRunner);
            }
        }

        private void ConnectNodes(string command)
        {
            var nodes = GetNodesForCommand(command);
            Broker.Connect(nodes.ToList());
        }

        private void DisconnectNodes(string command)
        {
            var nodes = GetNodesForCommand(command);
            Broker.Disconnect(nodes.ToList());
        }

        private static IEnumerable<string> GetNodesForCommand(string command)
        {
            var entries = command.Split(' ');
            var nodes = entries[1].Split(",").Select(x => x.Trim());
            return nodes;
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
            Console.WriteLine("disconnect - to disconnect one or more nodes");
            Console.WriteLine("connect - to reconnect one or more nodes");
            Console.WriteLine("timeout - to trigger and make one or more nodes candidates");
        }

        private void ConfigureLogging()
        {
            var config = new LoggingConfiguration();

            var fileTarget = CreateFileTarget();
            var consoleTarget = CreateConsoleTarget();
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            config.AddRuleForOneLevel(LogLevel.Debug, consoleTarget);
            config.AddRuleForOneLevel(LogLevel.Info, consoleTarget);
            config.AddRuleForOneLevel(LogLevel.Warn, consoleTarget);
            config.AddRuleForOneLevel(LogLevel.Error, consoleTarget);
            config.AddRuleForOneLevel(LogLevel.Fatal, consoleTarget);

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

        public NodeRunner InitializeLeader(string name)
        {
            var node = new Node(name, Broker) {Status = new LeaderStatus(0)};
            var timer = new ElectionTimer(TimeoutGenerator.GenerateElectionTimeout() * 10);
            var nodeRunner = new NodeRunner(node, timer, new StrategySelector(Nodes.Length));
            
            Broker.Register(nodeRunner);
            return nodeRunner;
        }

        public NodeRunner InitializeFollower(string name)
        {
            var node = new Node(name, Broker) {Status = new FollowerStatus(0)};
            var timer = new ElectionTimer(TimeoutGenerator.GenerateElectionTimeout() * 10);
            var nodeRunner = new NodeRunner(node, timer, new StrategySelector(Nodes.Length));
            Broker.Register(nodeRunner);
            return nodeRunner;
        }
    }
}