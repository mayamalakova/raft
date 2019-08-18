using System;
using System.Timers;

namespace Raft.Communication
{
    public interface ITimer
    {
        void Start();
        void Stop();
        void Reset();

        event ElapsedEventHandler Elapsed;
    }
}