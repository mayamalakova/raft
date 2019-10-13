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
        public string DisplayLog => string.Join(" ", Log.Select(x => x.Type == OperationType.Commit ? x.Value : $"{x.Value}(?)").ToArray());

        public Node(string name, IMessageBroker messageBroker)
        {
            Name = name;
            Broker = messageBroker;
        }

        public void UpdateLog(NodeMessage message, Guid entryId)
        {
            var logEntry = new LogEntry(OperationType.Update, message.Value, entryId, Status.Term);
            Log.Add(logEntry);
        }

        public void CommitLog(NodeMessage message)
        {
            var logEntry = LastLogEntry();
            if (logEntry?.Id != message.Id || logEntry.Type == OperationType.Commit)
            {
                return;
            }
            
            logEntry.Type = OperationType.Commit;
            Value = logEntry.Value;
        }

        public LogEntry LastLogEntry()
        {
            return Log.Count > 0 ? Log.Last() : null;
        }

        internal void ConfirmLogUpdate(Guid entryId)
        {
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
            var logUpdate = new NodeMessage(Status.Term, message.Value, MessageType.LogUpdate, Name, entryId);
            Broker.Broadcast(logUpdate);
        }

        private void SendVoteRequest(int term)
        {
            var lastLogEntry = LastLogEntry();
            var value = lastLogEntry == null ? "0,-1" : $"{lastLogEntry.Term},{Log.Count - 1}"; 
            var message = new NodeMessage(term, value, MessageType.VoteRequest, Name, Guid.NewGuid());
            Broker.Broadcast(message);
        }

        public void Vote(int term, string candidate, Guid electionId)
        {
            Logger.Debug($"{Name} votes for {candidate}");
            
            var voteMessage = new NodeMessage(term, candidate, MessageType.LeaderVote, Name, electionId);
            Broker.Broadcast(voteMessage);
        }

        public bool HasVotedInTerm(int term)
        {
            return Status.LastVote >= term;
        }

        public void SendPing()
        {
            var voteMessage = new NodeMessage(Status.Term, Name, MessageType.Info, Name, Guid.Empty);
                        Broker.Broadcast(voteMessage);
        }

        public void ResendVoteRequest()
        {
            var newTerm = Status.Term + 1;
            Logger.Debug($"{Name} resends vote request, term: {newTerm}");
            Status.Term = newTerm;
            SendVoteRequest(newTerm);
        }

        public void BecomeCandidate()
        {
            var newTerm = Status.Term + 1;

            Logger.Debug($"{Name} becomes candidate, term: {newTerm}");

            Status = new CandidateStatus(newTerm)
            {
                ConfirmedNodes = { Name }
            };
            SendVoteRequest(newTerm);
        }
    }
}