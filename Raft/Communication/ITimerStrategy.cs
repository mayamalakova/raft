using Raft.Entities;

namespace Raft.Communication
{
    public interface ITimerStrategy
    {
        void OnTimerElapsed();
        bool ShouldReset(NodeMessage message);
    }
}