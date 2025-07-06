using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class UserPromptParamModel
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("userMessage")]
        public string UserMessage { get; set; }
    }
}
