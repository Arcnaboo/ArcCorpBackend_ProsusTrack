using System;
using System.ComponentModel.DataAnnotations;
using ArcCorpBackend.Core.Users;


namespace ArcCorpBackend.Core.Messages
{
    
    public class Chat
    {
        [Key]
        public Guid ChatId { get; private set; }
        
        
        public DateTime CreatedAt { get; private set; }

        
        public string Name { get; private set; }

        
        public Guid UserId { get; private set; }

        


        public Chat(Guid userId, string name)
        {
            UserId = userId;
            ChatId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            
            Name = name;
        }

        // Parameterless constructor required for MessagePack deserialization
        private Chat()
        {

        }
    }
}
