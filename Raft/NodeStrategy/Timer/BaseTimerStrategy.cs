using Raft.Entities;

namespace Raft.NodeStrategy.Timer
{
    public class BaseTimerStrategy
    {
        protected static bool FromLeader(NodeMessage message)
        {
            return message.Type == MessageType.Info || message.Type == MessageType.LogUpdate ||
                   message.Type == MessageType.LogCommit;
        }

        protected static bool FromCandidate(NodeMessage message)
        {
            return message.Type == MessageType.VoteRequest;
        }
    }
}