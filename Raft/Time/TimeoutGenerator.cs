using System;

namespace Raft
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