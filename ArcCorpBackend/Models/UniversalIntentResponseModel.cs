using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class UniversalIntentResponseModel : CoreResultModel
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("hasCards")]
        public bool HasCards { get; set; }

        [JsonPropertyName("cards")]
        public List<CardModel> Cards { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("missingContext")]
        public List<string> MissingContext { get; set; }

        [JsonPropertyName("readyForAction")]
        public bool ReadyForAction { get; set; }
    }

    public class CardModel
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("rating")]
        public float? Rating { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("action")]
        public ActionModel Action { get; set; }
    }

    public class ActionModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
