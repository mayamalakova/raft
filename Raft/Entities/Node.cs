namespace Raft.Entities
{
    public class Node
    {
        private string Value { get; }
        public string Name { get; }

        public Node(string name, string value)
        {
            Value = value;
            Name = name;
        }

        public Node(string name)
        {
            Name = name;
        }
    }
}