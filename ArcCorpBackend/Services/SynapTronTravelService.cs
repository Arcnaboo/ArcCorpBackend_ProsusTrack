using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class SynapTronTravelService
    {
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "GkKrT~ky.YfOof>?\\uT%Mpl,>2,ged~nMAVyOB&<^`G?2XXpG@_Ad@=k";

        public static readonly SynapTronTravelService Instance = new();

        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;
        private readonly List<Dictionary<string, string>> _globalHistory;
        private readonly string _systemFacts;

        private SynapTronTravelService()
        {
            var enigma = new Enigma3Service();
            _groqApiKey = enigma.Decrypt(ApiKeyGuid, EncryptedApiKey);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

            _globalHistory = new List<Dictionary<string, string>>();

            _systemFacts =
            "You are a backend travel assistant AI specialized only in Flight Booking requests.\n" +
            "You NEVER greet or speak casually. You are not a chatbot.\n\n" +
            "🎯 Your task is:\n" +
            "1. Read the user message in the format {userMessage: \"...\"}.\n" +
            "2. Extract the required fields: fromId, toId, departureDate.\n" +
            "3. If any of these fields are missing, set readyForAction:false, list the missing fields in missingContext, and generate a polite question in userPrompt asking for the missing details.\n" +
            "4. If all required fields are present, set readyForAction:true and output the finalized travel details as a plain English summary in userPrompt. NEVER leave userPrompt null or empty when readyForAction is true.\n\n" +
            "✅ Output Format:\n" +
            "{\n" +
            "  \"readyForAction\": true or false,\n" +
            "  \"category\": \"Flight Booking\",\n" +
            "  \"missingContext\": [\"fromId\", \"toId\", \"departureDate\"],\n" +
            "  \"userPrompt\": \"if ready, the finalized user travel details as a summary string; otherwise, a polite follow-up question. Never null.\",\n" +
            "  \"fromId\": \"IATA code of the departure airport, e.g., IST\",\n" +
            "  \"toId\": \"IATA code of the arrival airport, e.g., ESB\",\n" +
            "  \"departureDate\": \"yyyy-MM-dd\"\n" +
            "}\n";

            _globalHistory.Add(new()
            {
                ["role"] = "system",
                ["content"] = _systemFacts
            });
        }

        public async Task<SynapTronResponse> CategorizeIntent(string userMessage)
        {
            var prompt = $"{{userMessage: \"{userMessage}\"}}";
            _globalHistory.Add(new() { ["role"] = "user", ["content"] = prompt });

            try
            {
                string rawJson = await PostChatAsync(_globalHistory);

                rawJson = rawJson.Trim();
                if (rawJson.StartsWith("```json")) rawJson = rawJson[7..];
                if (rawJson.EndsWith("```")) rawJson = rawJson[..^3];
                rawJson = rawJson.Trim();

                _globalHistory.Add(new() { ["role"] = "assistant", ["content"] = rawJson });

                var result = JsonSerializer.Deserialize<IntentCategorizationResult>(rawJson);
                var arc = result ?? new IntentCategorizationResult
                {
                    ReadyForAction = false,
                    Category = "Uncategorized",
                    MissingContext = new() { "Groq failed to respond properly" },
                    UserPrompt = "Sorry, I couldn’t understand. Please try rephrasing."
                };

                if (arc.ReadyForAction && arc.Category.Equals("Flight Booking", StringComparison.OrdinalIgnoreCase))
                {
                    var flightParams = new SearchFlightParams
                    {
                        Source = arc.Source ?? "City:LON",
                        Destination = arc.Destination ?? "City:DXB",
                        OutboundDepartmentDateStart = arc.DepartureDate ?? "2025-08-10T00:00:00",
                        OutboundDepartmentDateEnd = arc.DepartureDate ?? "2025-08-10T00:00:00",
                    };

                    var rawFlightJson = await BookingComAPIService.Instance.SearchFlightAsyncKiwi(flightParams);

                    var handler = new BookingComResultHandlerService();
                    var cards = handler.Handle(rawFlightJson);

                    if (cards.Count == 0)
                    {
                        return new SynapTronResponse
                        {
                            Success = true,
                            Category = arc.Category,
                            HasCards = false,
                            Cards = new List<Card>(),
                            Message = "No flights found.",
                            MissingContext = arc.MissingContext,
                            ReadyForAction = true
                        };
                    }

                    return new SynapTronResponse
                    {
                        Success = true,
                        Category = arc.Category,
                        HasCards = true,
                        Cards = cards,
                        Message = $"{cards.Count} flights found.",
                        MissingContext = arc.MissingContext,
                        ReadyForAction = true
                    };
                }

                return new SynapTronResponse
                {
                    Success = true,
                    Category = arc.Category,
                    HasCards = false,
                    Cards = new List<Card>(),
                    Message = arc.UserPrompt ?? "No prompt generated.",
                    MissingContext = arc.MissingContext,
                    ReadyForAction = arc.ReadyForAction
                };
            }
            catch (Exception ex)
            {
                return new SynapTronResponse
                {
                    Success = false,
                    Category = "Error",
                    HasCards = false,
                    Cards = new List<Card>(),
                    Message = $"⚠️ Internal Error: {ex.Message}",
                    MissingContext = new(),
                    ReadyForAction = false
                };
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
    }

    public class IntentCategorizationResult
    {
        [JsonPropertyName("readyForAction")]
        public bool ReadyForAction { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("missingContext")]
        public List<string> MissingContext { get; set; } = new();

        [JsonPropertyName("userPrompt")]
        public string? UserPrompt { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("departureDate")]
        public string? DepartureDate { get; set; }
    }


    public class SynapTronResponse
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
}
