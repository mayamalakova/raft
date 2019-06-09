using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Raft.Entities
{
    public class Node
    {
        public string Value { get; set; }
        public string Name { get; }
        private IEnumerable<LogEntry> Log { get; }

        public Node(string name, string value)
        {
            Value = value;
            Name = name;
            Log = new List<LogEntry>();
        }

        public Node(string name)
        {
            Name = name;
        }
    }
}