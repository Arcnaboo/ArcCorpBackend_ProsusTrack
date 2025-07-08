using System;
using System.ComponentModel.DataAnnotations;


namespace ArcCorpBackend.Core.Users
{
    
    public  class UserData
    {
        [Key]
        public Guid UserDataId { get; private set; }

 
        public Guid UserId { get; private set; }

        
        public string Message { get; private set; }
     

        public UserData(Guid userid, string message)
        {
            UserId = userid;
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            UserDataId = Guid.NewGuid();
            Message = message;
        }

        // Parameterless constructor required for MessagePack deserialization
        private UserData() { }
    }
}
