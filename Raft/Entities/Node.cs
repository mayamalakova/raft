using System;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using Raft.Election;

namespace Raft.Entities
{
    public class Node
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int _lastTerm = 0;

        private IMessageBroker Broker { get; }

        public string Name { get; }
        
        public Collection<LogEntry> Log { get; } = new Collection<LogEntry>();

        public string Value { get; private set; }
        public NodeStatus Status { get; set; }

        public Node(string name, string value)
        {
            Value = value;
            Name = name;
        }

        public Node(string name, IMessageBroker messageBroker)
        {
            Name = name;
            Broker = messageBroker;
        }

        public void UpdateLog(NodeMessage message, Guid entryId)
        {
            var logEntry = new LogEntry(OperationType.Update, message.Value, entryId);
            Log.Add(logEntry);
        }

        public void CommitLog(NodeMessage message)
        {
            var logEntry = LastLogEntry();
            if (logEntry?.Id != message.Id || logEntry.Type == OperationType.Commit)
            {
                return;
            }
            Logger.Debug($"{Name} committing {message.Id}");
            
            logEntry.Type = OperationType.Commit;
            Value = logEntry.Value;
        }

        protected internal LogEntry LastLogEntry()
        {
            return Log.Count > 0 ? Log.Last() : null;
        }

        internal void ConfirmLogUpdate(Guid entryId)
        {
            Logger.Debug($"node {Name} confirms {entryId}");
            
            var nodeMessage = new NodeMessage(null, MessageType.LogUpdateConfirmation, Name, entryId);
            Broker.Broadcast(nodeMessage);
        }

        internal void SendCommit(NodeMessage message)
        {
            var nodeMessage = new NodeMessage(message.Value, MessageType.LogCommit, Name, message.Id);
            Broker.Broadcast(nodeMessage);
        }

        internal void SendLogUpdateRequest(NodeMessage message, Guid entryId)
        {
            Logger.Debug($"{Name} initiating update {entryId}");
            
            var logUpdate = new NodeMessage(message.Value, MessageType.LogUpdate, Name, entryId);
            Broker.Broadcast(logUpdate);
        }

        public void SendVoteRequest()
        {
            _lastTerm++;
            var message = new NodeMessage(_lastTerm.ToString(), MessageType.VoteRequest, Name, Guid.NewGuid());
            Broker.Broadcast(message);
        }

        public void Vote(string leader, Guid electionId)
        {
            var voteMessage = new NodeMessage(leader, MessageType.LeaderVote, Name, electionId);
            Broker.Broadcast(voteMessage);
        }

        public bool HasVotedInTerm(int term)
        {
            return _lastTerm >= term;
        }
    }
}