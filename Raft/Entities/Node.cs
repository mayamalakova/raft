using System;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;

namespace Raft.Entities
{
    public class Node
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IMessageBroker Broker { get; }

        public string Name { get; }
        
        public Collection<LogEntry> Log { get; } = new Collection<LogEntry>();

        public string Value { get; private set; }
        
        public INodeStatus Status { get; set; }

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

        public LogEntry LastLogEntry()
        {
            return Log.Count > 0 ? Log.Last() : null;
        }

        internal void ConfirmLogUpdate(Guid entryId)
        {
            Logger.Debug($"node {Name} confirms {entryId}");
            
            var nodeMessage = new NodeMessage(Status.Term, null, MessageType.LogUpdateConfirmation, Name, entryId);
            Broker.Broadcast(nodeMessage);
        }

        internal void SendCommit(NodeMessage message)
        {
            var nodeMessage = new NodeMessage(Status.Term, message.Value, MessageType.LogCommit, Name, message.Id);
            Broker.Broadcast(nodeMessage);
        }

        internal void SendLogUpdateRequest(NodeMessage message, Guid entryId)
        {
            Logger.Debug($"{Name} initiating update {entryId}");
            
            var logUpdate = new NodeMessage(Status.Term, message.Value, MessageType.LogUpdate, Name, entryId);
            Broker.Broadcast(logUpdate);
        }

        public void SendVoteRequest(int term)
        {
            var message = new NodeMessage(term, Name, MessageType.VoteRequest, Name, Guid.NewGuid());
            Broker.Broadcast(message);
        }

        public void Vote(int term, string leader, Guid electionId)
        {
            var voteMessage = new NodeMessage(term, leader, MessageType.LeaderVote, Name, electionId);
            Broker.Broadcast(voteMessage);
        }

        public bool HasVotedInTerm(int term)
        {
            return Status.Term >= term;
        }
    }
}