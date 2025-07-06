using System;
using ArcCorpBackend.Core.Users;
using MessagePack;

namespace ArcCorpBackend.Core.Messages
{
    [MessagePackObject]
    public partial class Chat
    {
        [Key(0)]
        public Guid ChatId { get; private set; }

        [Key(1)]
        public DateTime CreatedAt { get; private set; }

        [Key(2)]
        public User User { get; private set; }

        [Key(3)]
        public List<Message> Messages { get; private set; }



        public Chat(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            ChatId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Messages = new List<Message>();
        }

        // Parameterless constructor required for MessagePack deserialization
        private Chat() 
        {
            if (Messages == null)
            {
                Messages = new List<Message>();
            }
        }
    }
}
