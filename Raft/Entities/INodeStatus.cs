namespace Raft.Entities
{
    public interface INodeStatus
    {
        string Name { get; }

        int Term { get; set; }
    }
}