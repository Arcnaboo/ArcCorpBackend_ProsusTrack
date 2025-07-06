using System;
using MessagePack;

namespace ArcCorpBackend.Core.Messages
{
    [MessagePackObject]
    public partial class Message
    {
        [Key(0)]
        public Guid MessageId { get; private set; }

        [IgnoreMember] // Prevent recursive serialization back to Chat
        public Chat Chat { get; private set; }

        [Key(2)]
        public DateTime CreatedAt { get; private set; }

        [Key(3)]
        public bool IsUserMessage { get; private set; }

        [Key(4)]
        public string Content { get; private set; }

        public Message(Chat chat, bool isUserMessage, string content)
        {
            Chat = chat ?? throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or empty.", nameof(content));

            MessageId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsUserMessage = isUserMessage;
            Content = content;
        }

        // Parameterless constructor required for MessagePack deserialization
        private Message() { }
    }
}
