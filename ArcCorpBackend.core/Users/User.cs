using System;
using System.ComponentModel.DataAnnotations;
using ArcCorpBackend.Core.Messages;


namespace ArcCorpBackend.Core.Users
{
    
    public  class User
    {
        [Key]
        public Guid UserId { get; private set; }

        public string Email { get; private set; }

        public List<Chat> Chats { get; private set; }

        public User(Guid userId, string email, List<Chat> chats)
        {
            UserId = userId;
            Email = email;
            Chats = chats;
        }

        public User(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            UserId = Guid.NewGuid();
            Email = email;
            Chats = new List<Chat>();
        }

        // Parameterless constructor required for MessagePack deserialization
        private User()
        {
            if (Chats == null)
            {
                Chats = new List<Chat>();
            }
        }
    }
}
