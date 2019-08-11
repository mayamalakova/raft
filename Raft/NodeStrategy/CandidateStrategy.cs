using System;
using System.Collections.Generic;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Candidate node strategy for responding to Raft messages
    /// </summary>
    public class CandidateStrategy : IMessageResponseStrategy
    {
        private readonly Node _node;
        private readonly int _nodesCount;
        private int _term;
        private readonly HashSet<string> _votes;

        public CandidateStrategy(Node node, int nodesCount, int term)
        {
            _node = node;
            _nodesCount = nodesCount;
            _term = term;
            var candidateStatus = _node.Status as CandidateStatus;
            _votes = candidateStatus?.ConfirmedNodes;
        }

        public void RespondToMessage(NodeMessage message)
        {
            switch (message.Type)
            {
                case MessageType.LogUpdate:
                    BecomeFollower(message);
                    break;

                case MessageType.LogUpdateConfirmation:
                    BecomeFollower(message);
                    break;

                case MessageType.LogCommit:
                    BecomeFollower(message);
                    break;

                case MessageType.ValueUpdate:
                    break;
                
                case MessageType.Info:
                    BecomeFollower(message);
                    break;
                
                case MessageType.VoteRequest:
                    if (message.Term > _node.Status.Term)
                    {
                        BecomeFollower(message);
                        _node.Vote(message.Term, message.SenderName, message.Id);
                    }
                    break;
                
                case MessageType.LeaderVote:
                    if (message.Term > _node.Status.Term)
                    {
                        BecomeFollower(message);
                        break;
                    }
                    
                    AddVote(message);
                    if (HasMajority())
                    {
                        BecomeLeader();
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddVote(NodeMessage message)
        {
            _votes.Add(message.SenderName);
        }

        private void BecomeLeader()
        {
            _node.Status = new LeaderStatus(_node.Status.Term);
        }

        private bool HasMajority()
        {
            return _votes.Count > _nodesCount / 2;
        }

        private void BecomeFollower(NodeMessage message)
        {
            _node.Status = new FollowerStatus(message.Term);
        }
    }
}