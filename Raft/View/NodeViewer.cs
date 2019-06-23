using System;
using Raft.Entities;

namespace Raft.View
{
    public class NodeViewer: INodeSubscriber
    {
        
        public void NodeValueChanged(string name, string value)
        {
            Console.WriteLine($"{name} : {value}");
        }
    }
}