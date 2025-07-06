using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class ChatResultModel : CoreResultModel
    {
        [JsonPropertyName("chat")]
        public ChatModel ChatModel { get; set; }
    }
}
