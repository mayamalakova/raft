using NLog;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    public abstract class BaseStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Node Node;
        
        protected void BecomeLeader()
        {
            Logger.Debug($"{Node.Name} becomes leader, term: {Node.Status.Term}");

            Node.Status = new LeaderStatus(Node.Status.Term);
            Node.SendPing();
        }

        protected void BecomeFollower(NodeMessage message)
        {
            Logger.Debug($"{Node.Name} becomes follower of {message.SenderName}, term: {message.Term}");

            Node.Status = new FollowerStatus(message.Term);
        }

        protected void ConfirmLogUpdate(NodeMessage message)
        {
            Node.UpdateLog(message, message.Id);
            Node.ConfirmLogUpdate(message.Id, message.SenderName);
        }

        protected void CommitLog(NodeMessage message)
        {
            Node.CommitLog(message);
        }

        protected bool CandidateIsUpToDate(NodeMessage message)
        {
            if (message.Type != MessageType.VoteRequest || Node.LastLogEntry() == null)
            {
                return true;
            }
            
            var (term, index) = ParseLastLogEntryInfo(message);
            return term >= Node.LastLogEntry().Term && (term != Node.LastLogEntry().Term || index >= Node.Log.Count - 1);
        }

        private static (int term, int index) ParseLastLogEntryInfo(NodeMessage message)
        {
            var lastLogEntryInfo = message.Value.Split(",");
            return (int.Parse(lastLogEntryInfo[0]), int.Parse(lastLogEntryInfo[1]));
        }

        protected virtual bool IsFromOlderTerm(NodeMessage message)
        {
            return message.Term < Node.Status.Term;
        }
    }
}