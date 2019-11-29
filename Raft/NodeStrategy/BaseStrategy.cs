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
    }
}