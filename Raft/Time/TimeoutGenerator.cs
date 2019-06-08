using System;

namespace Raft.Time
{
    public class TimeoutGenerator
    {
        public int GenerateElectionTimeout()
        {
            var random = new Random();
            return random.Next(150, 300);
        }
    }
}