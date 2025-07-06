using ArcCorpBackend.Core.Users;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class UserModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("chats")]
        public List<ChatModel> Chats { get; set; }

        public UserModel(User user)
        {
            Email = user.Email;
            Chats = new List<ChatModel>();
            if (user.Chats != null)
            {
                foreach (var chat in user.Chats)
                {
                    Chats.Add(new ChatModel(chat));
                }
            }
        }
    }
}
