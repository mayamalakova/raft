using System;

namespace Raft.Entities
{
    public class NodeMessage
    {
        public string Value { get; }
        public MessageType Type { get; }

        public string SenderName { get; }

        public int Term { get; }

        public Guid Id { get; }

        public string  Recipient { get; }
        
        public NodeMessage(int term, string value, MessageType type, string senderName, Guid id, string recipient = null)
        {
            Value = value;
            Type = type;
            SenderName = senderName;
            Id = id;
            Recipient = recipient;
            Term = term;
        }
    }

    public enum MessageType
    {
        ValueUpdate, // client appends log entry
        LogUpdate,  // leader updates the log
        LogUpdateConfirmation, // follower accepts log update
        LogCommit, // leader commits a log entry
        Info, // heartbeat
        VoteRequest, // candidate initiates election
        LeaderVote // follower votes for candidate
    }
}