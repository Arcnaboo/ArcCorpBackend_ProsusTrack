using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class LoginResultModel : CoreResultModel
    {
        [JsonPropertyName("jwtAuthKey")]
        public string JwtAuthKey { get; set; }
    }
}
