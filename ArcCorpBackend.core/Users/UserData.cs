using System;
using MessagePack;

namespace ArcCorpBackend.Core.Users
{
    [MessagePackObject]
    public partial class UserData
    {
        [Key(0)]
        public Guid Id { get; private set; }

        [Key(1)]
        public User User { get; private set; }

        [Key(2)]
        public string Message { get; private set; }

        public UserData(User user, string message)
        {
            User = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            Id = Guid.NewGuid();
            Message = message;
        }

        // Parameterless constructor required for MessagePack deserialization
        private UserData() { }
    }
}
