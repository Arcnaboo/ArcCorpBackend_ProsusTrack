using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class IntentCategorizationResult
    {
        [JsonPropertyName("readyForAction")]
        public bool ReadyForAction { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("missingContext")]
        public List<string> MissingContext { get; set; } = new();

        [JsonPropertyName("userPrompt")]
        public string? UserPrompt { get; set; }
    }

    public class UniversalIntentResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("hasCards")]
        public bool HasCards { get; set; }

        [JsonPropertyName("cards")]
        public List<Card> Cards { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("missingContext")]
        public List<string> MissingContext { get; set; } = new();

        [JsonPropertyName("readyForAction")]
        public bool ReadyForAction { get; set; }
    }

    public class Card
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; } = "";

        [JsonPropertyName("rating")]
        public float? Rating { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        [JsonPropertyName("details")]
        public string Details { get; set; } = "";

        [JsonPropertyName("action")]
        public ActionObj Action { get; set; } = new();
    }

    public class ActionObj
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }

    public class ApiPayloadDto
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = "";

        [JsonPropertyName("checkIn")]
        public string CheckIn { get; set; } = "";

        [JsonPropertyName("checkOut")]
        public string CheckOut { get; set; } = "";

        [JsonPropertyName("adults")]
        public int Adults { get; set; } = 1;
    }

    public class SynapTronTravelService
    {
        private static SynapTronTravelService? _instance = null;
        private readonly HttpClient _httpClient;
        private readonly List<Dictionary<string, string>> _globalHistory;
        private readonly string _systemFacts;
        private readonly string _groqApiKey = "YOUR_GROQ_API_KEY";
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "GkKrT~ky.YfOof>?\\uT%Mpl,>2,ged~nMAVyOB&<^`G?2XXpG@_Ad@=k";


        private SynapTronTravelService()
        {
            var enigma = new Enigma3Service();
            _groqApiKey = enigma.Decrypt(ApiKeyGuid, EncryptedApiKey);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

            var allCategories = new CategoryService().GetAllCategories();
            var allCatsAsBulletList = string.Join("\n- ", allCategories);

            _systemFacts =
                "You are SynapTron, a backend AI engine classifying travel requests.\n" +
                "NEVER greet or chat casually; you are not a chatbot.\n\n" +
                "🎯 Task:\n" +
                "1. Read the message: {userMessage: \"...\"}\n" +
                "2. Pick the best category.\n" +
                "3. Compare message to required context.\n" +
                "4. Identify missing fields.\n" +
                "5. If all provided, set readyForAction:true and provide summary in userPrompt.\n\n" +
                "📦 Categories:\n- " + allCatsAsBulletList + "\n\n" +
                "✅ Output format:\n" +
                "{\n  \"readyForAction\": true/false,\n  \"category\": \"...\",\n  \"missingContext\": [...],\n  \"userPrompt\": \"...\"\n}\n" +
                "Respond strictly in JSON. No explanation or markdown.";

            _globalHistory = new List<Dictionary<string, string>>
            {
                new() { ["role"] = "system", ["content"] = _systemFacts }
            };
        }

        public static SynapTronTravelService Instance
        {
            get
            {
                _instance ??= new SynapTronTravelService();
                return _instance;
            }
        }

        private async Task<string> PostChatAsync(List<Dictionary<string, string>> history)
        {
            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = history
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Groq API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var completion = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            return completion.Trim();
        }

        public async Task<string> CategorizeIntent(string userMessage)
        {
            var prompt = $"{{userMessage: \"{userMessage}\"}}";
            _globalHistory.Add(new() { ["role"] = "user", ["content"] = prompt });

            try
            {
                string rawJson = await PostChatAsync(_globalHistory);

                if (rawJson.StartsWith("```json")) rawJson = rawJson[7..];
                if (rawJson.EndsWith("```")) rawJson = rawJson[..^3];

                _globalHistory.Add(new() { ["role"] = "assistant", ["content"] = rawJson });

                var result = JsonSerializer.Deserialize<IntentCategorizationResult>(rawJson);
                var arc = result ?? new IntentCategorizationResult
                {
                    ReadyForAction = false,
                    Category = "Uncategorized",
                    MissingContext = new() { "Groq failed to respond" },
                    UserPrompt = "Sorry, I couldn’t understand. Please rephrase."
                };

                return arc.ReadyForAction
                    ? await HandleReadyForAction(arc.Category, arc.UserPrompt ?? "")
                    : arc.UserPrompt ?? "Error: null userPrompt";
            }
            catch (Exception ex)
            {
                return $"⚠️ Internal Error: {ex.Message}";
            }
        }

        private async Task<string> HandleReadyForAction(string category, string finalizedContext)
        {
            string payloadJson = await GenerateApiPayloadFromContext(finalizedContext);
            var payload = JsonSerializer.Deserialize<ApiPayloadDto>(payloadJson) ?? throw new Exception("Payload parsing failed");

            UniversalIntentResponse responseDto;
            var service = ExpediaService.Instance;

            switch (category)
            {
                case "Hotel Booking":
                    var hotelsJson = await service.SearchPropertiesAsync(payload.City, payload.CheckIn, payload.CheckOut, payload.Adults);
                    var hotels = JsonSerializer.Deserialize<List<Card>>(hotelsJson) ?? new();

                    responseDto = new()
                    {
                        Success = true,
                        Category = category,
                        HasCards = hotels.Count > 0,
                        Cards = hotels,
                        Message = hotels.Count > 0
                            ? $"Found {hotels.Count} hotels in {payload.City}."
                            : $"No hotels available in {payload.City}.",
                        ReadyForAction = true
                    };
                    break;

                case "Flight Booking":
                    var flightsJson = await service.SearchFlightsAsync(payload.City, payload.City, payload.CheckIn, payload.CheckOut, payload.Adults);
                    var flights = JsonSerializer.Deserialize<List<Card>>(flightsJson) ?? new();

                    responseDto = new()
                    {
                        Success = true,
                        Category = category,
                        HasCards = flights.Count > 0,
                        Cards = flights,
                        Message = flights.Count > 0
                            ? $"Found {flights.Count} flights."
                            : "No flights available for your query.",
                        ReadyForAction = true
                    };
                    break;

                case "Tour Booking":
                    var toursJson = await service.SearchToursAsync(payload.City, payload.CheckIn, payload.CheckOut);
                    var tours = JsonSerializer.Deserialize<List<Card>>(toursJson) ?? new();

                    responseDto = new()
                    {
                        Success = true,
                        Category = category,
                        HasCards = tours.Count > 0,
                        Cards = tours,
                        Message = tours.Count > 0
                            ? $"Found {tours.Count} tours in {payload.City}."
                            : $"No tours available in {payload.City}.",
                        ReadyForAction = true
                    };
                    break;

                default:
                    responseDto = new()
                    {
                        Success = false,
                        Category = category,
                        HasCards = false,
                        Cards = new(),
                        Message = $"Unsupported category: {category}.",
                        ReadyForAction = false
                    };
                    break;
            }

            return JsonSerializer.Serialize(responseDto);
        }

        private async Task<string> GenerateApiPayloadFromContext(string finalizedContext)
        {
            var payloadHistory = new List<Dictionary<string, string>>
            {
                new() { ["role"] = "system", ["content"] =
                    "You are an API payload generator. Given the finalized travel details, produce minimal JSON payload for booking API. Respond ONLY with pure JSON." },
                new() { ["role"] = "user", ["content"] = finalizedContext }
            };

            return await PostChatAsync(payloadHistory);
        }
    }
}
