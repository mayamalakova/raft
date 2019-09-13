using System.Timers;

namespace Raft.Communication
{
    public interface ITimer
    {
        void Start();
        void Reset();

        event ElapsedEventHandler Elapsed;
    }
}