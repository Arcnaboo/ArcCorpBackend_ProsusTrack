using System;
using System.ComponentModel.DataAnnotations;


namespace ArcCorpBackend.Core.Messages
{
    
    public  class Message
    {
        [Key]
        public Guid MessageId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        
        public bool IsUserMessage { get; private set; }

        public Guid ChatId { get; private set; }
        
        public string Content { get; private set; }

        public Message(Guid messageId, Guid ChatId, DateTime createdAt, bool isUserMessage, string content)
        {
            MessageId = messageId;
            CreatedAt = createdAt;
            IsUserMessage = isUserMessage;
            Content = content;
        }

        public Message(Guid chatId, bool isUserMessage, string content)
        {
            
            ChatId = chatId;
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
