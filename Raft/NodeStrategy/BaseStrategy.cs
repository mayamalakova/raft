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
        }

        protected void BecomeFollower(NodeMessage message)
        {
            Logger.Debug($"{Node.Name} becomes follower, term: {Node.Status.Term}");
            
            Node.Status = new FollowerStatus(message.Term);
        }

        protected void ConfirmLogUpdate(NodeMessage message)
        {
            Node.UpdateLog(message, message.Id);
            Node.ConfirmLogUpdate(message.Id);
        }
    }
}