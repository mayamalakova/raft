using Raft.Entities;

namespace Raft.NodeStrategy
{
    public abstract class BaseStrategy
    {
        protected Node Node;

        protected void BecomeLeader()
        {
            Node.Status = new LeaderStatus(Node.Status.Term);
        }

        protected void BecomeFollower(NodeMessage message)
        {
            Node.Status = new FollowerStatus(message.Term);
        }
    }
}