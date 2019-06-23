using System;
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
                    Console.WriteLine("You can use the following commands: " +
                                      "value - to enter new value, /n" +
                                      "disconnect - to disconnect a node, /n" +
                                      "connect - to reconnect a node");
                    
                } else if (command.StartsWith("value"))
                {
                    var entries = command.Split(' ');
                    var value = entries[1];
                    _messageBroker.Broadcast(value);
                    
                } else if (command.StartsWith("disconnect"))
                {
                    var entries = command.Split(' ');
                    _messageBroker.Disconnect(entries[1]);
                    
                } else if (command.StartsWith("connect"))
                {
                    var entries = command.Split(' ');
                    _messageBroker.Connect(entries[1]);
                }
                else
                {
                    Console.WriteLine("Write ? and press Enter to see the options, or press Enter to exit");
                }
            }
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
            var task = new Task(() =>
            {
                var nodeRunner = new LeaderNodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
                nodeRunner.Subscribe(new NodeViewer());
                nodeRunner.Start();
            });
            task.Start();
        }

        private void StartNode(string name)
        {
            var task = new Task(() =>
            {
                var nodeRunner = new NodeRunner(name, TimeoutGenerator.GenerateElectionTimeout(), _messageBroker);
                nodeRunner.Subscribe(new NodeViewer());
                nodeRunner.Start();
            });
            task.Start();
        }
    }
}