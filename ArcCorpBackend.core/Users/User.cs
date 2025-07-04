using System;
using MessagePack;

namespace ArcCorpBackend.Core.Users
{
    [MessagePackObject]
    public partial class User
    {
        [Key(0)]
        public Guid UserId { get; private set; }

        [Key(1)]
        public string Email { get; private set; }

        public User(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            UserId = Guid.NewGuid();
            Email = email;
        }

        // Parameterless constructor required for MessagePack deserialization
        private User() { }
    }
}
