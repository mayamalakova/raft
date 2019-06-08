using System;
using System.Collections.ObjectModel;
using Raft.Communication;
using Raft.Entities;

namespace Raft
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello Raft!");
            
            new RaftRunner().Run();

            Console.WriteLine("Bye Raft!");

        }
    }
}