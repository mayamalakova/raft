using System;
using NSubstitute;
using NUnit.Framework;
using Raft.Election;
using Raft.Entities;
using Shouldly;

namespace Raft.Test.Election
{
    [TestFixture]
    public class LeaderNodeShould
    {
        private const string TestValue = "test-message";
        
        private LeaderNodeRunner _leaderNodeRunner;
        private IMessageBroker _messageBroker;

        [SetUp]
        public void SetUp()
        {
            _messageBroker = Substitute.For<IMessageBroker>();
            _leaderNodeRunner = new LeaderNodeRunner("test", 100, _messageBroker);
        }
        
        [Test]
        public void UpdateLog_OnValueUpdateReceived()
        {
            var message = new NodeMessage(TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _leaderNodeRunner.Log.Count.ShouldBe(1);
            _leaderNodeRunner.Log[0].Value.ShouldBe(TestValue);
            _leaderNodeRunner.Log[0].Type.ShouldBe(OperationType.Update);
            
        }
        
        [Test]
        public void SendLogUpdate_OnValueUpdateReceived()
        {
            var message = new NodeMessage(TestValue, MessageType.ValueUpdate, null, Guid.Empty);
            _leaderNodeRunner.ReceiveMessage(message);
            
            _messageBroker.Received(1).Broadcast(Arg.Is<NodeMessage>(m => m.Type == MessageType.LogUpdate));
            
        }
    }
}