using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using Raft.Communication;
using Raft.Election;
using Raft.Time;
using Raft.View;

namespace Raft.Runner
{
    public class RaftRunner
    {
        private static readonly TimeoutGenerator TimeoutGenerator = new TimeoutGenerator();
        private readonly IMessageBroker _messageBroker = new MessageBroker();
        private readonly Collection<IMessageBrokerListener> _nodeRunners = new Collection<IMessageBrokerListener>();

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
            _messageBroker.Connect(entries[1]);
        }

        private void DisconnectNode(string command)
        {
            var entries = command.Split(' ');
            _messageBroker.Disconnect(entries[1]);
        }

        private void UpdateValue(string command)
        {
            var entries = command.Split(' ');
            var value = entries[1];
            _messageBroker.Broadcast(value);
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
            var nodeRunner = new LeaderNodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            _nodeRunners.Add(nodeRunner);
            var task = new Task(() =>
            {
//                nodeRunner.Subscribe(new NodeViewer());
                nodeRunner.Start();
            });
            task.Start();
        }

        private void StartNode(string name)
        {
            var nodeRunner = new NodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
            _nodeRunners.Add(nodeRunner);
            var task = new Task(() =>
            {
//                nodeRunner.Subscribe(new NodeViewer());
                nodeRunner.Start();
            });
            task.Start();
        }
    }
}