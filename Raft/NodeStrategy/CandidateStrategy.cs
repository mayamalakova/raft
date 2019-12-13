using System;
using System.Collections.Generic;
using NLog;
using Raft.Communication;
using Raft.Entities;

namespace Raft.NodeStrategy
{
    /// <summary>
    /// Candidate node strategy for responding to Raft messages
    /// </summary>
    public class CandidateStrategy : BaseStrategy, IMessageResponseStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _nodesCount;
        private readonly HashSet<string> _votes;

        public CandidateStrategy(Node node, int nodesCount)
        {
            Node = node;
            _nodesCount = nodesCount;
            var candidateStatus = Node.Status as CandidateStatus;
            _votes = candidateStatus?.ConfirmedNodes;
        }

        public void RespondToMessage(NodeMessage message)
        {
            if (IsFromOlderTerm(message))
            {
                return;
            }
            
            if (message.Term > Node.Status.Term)
            {
                BecomeFollower(message);
                new FollowerStrategy(Node).RespondToMessage(message);
                return;
            }
            
            switch (message.Type)
            {
                case MessageType.LeaderVote:
                    if (message.Value != Node.Name)
                    {
                        break;
                    }

                    AddVote(message);
                    if (HasMajority())
                    {
                        Logger.Debug($"{Node.Name} has majority {_votes.Count} / {_nodesCount}");
                        BecomeLeader();
                    }

                    break;
                case MessageType.ValueUpdate:
                    break;
                case MessageType.LogUpdate:
                    break;
                case MessageType.LogUpdateConfirmation:
                    break;
                case MessageType.LogCommit:
                    break;
                case MessageType.Info:
                    break;
                case MessageType.VoteRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void AddVote(NodeMessage message)
        {
            _votes.Add(message.SenderName);
        }

        private bool HasMajority()
        {
            return _votes.Count > _nodesCount / 2;
        }
    }
}