using Raft.Entities;

namespace Raft.Communication
{
    /// <summary>
    /// Defines the strategy to follow when replying to Raft messages
    /// </summary>
    public interface IMessageResponseStrategy
    {
        void RespondToMessage(NodeMessage message);
    }
}