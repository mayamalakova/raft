using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Entities;
using Raft.NodeStrategy;

namespace Raft.Test.Strategy
{
    [TestFixture]
    public class LeaderStrategyShould
    {
        [Test]
        public void AskForLogUpdate_WhenClientRequestsValueUpdate()
        {
            var messageBroker = Substitute.For<IMessageBroker>();
            var node = new Node("L", messageBroker) {Status = new LeaderStatus(0)};
            var leaderStrategy = new LeaderStrategy(node, 3);
            
            leaderStrategy.RespondToMessage(new NodeMessage(0, "new value", MessageType.ValueUpdate, null, Guid.Empty));
            
            messageBroker.Received(1).Broadcast(
                Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate && 
                m.SenderName == "L"));
        }
    }
}