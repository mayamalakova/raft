using Raft.Entities;

namespace Raft.Election
{
    public interface IMessageResponseStrategy
    {
        void RespondToMessage(NodeMessage message);
    }
}