using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class CoreResultModel
    {
        [JsonPropertyName("sucessful")]
        public bool Sucessful { get; set; }
    }
}
