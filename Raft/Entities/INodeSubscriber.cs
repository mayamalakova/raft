namespace Raft.Entities
{
    public interface INodeSubscriber
    {
        void NodeValueChanged(string name, string value);
    }
}