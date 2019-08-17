using System.Timers;
using Raft.Communication;

namespace Raft.Time
{
    public class ElectionTimer: ITimer
    {

        private readonly Timer _timer;

        public ElectionTimer(int interval)
        {
            _timer = new Timer(interval);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Reset()
        {
            _timer.Stop();
            _timer.Start();
        }

        public event ElapsedEventHandler Elapsed
        {
            add => _timer.Elapsed += value;
            remove => _timer.Elapsed -= value;
        }
    }
}