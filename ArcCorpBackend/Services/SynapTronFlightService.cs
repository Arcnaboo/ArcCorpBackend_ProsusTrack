using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public sealed class SynapTronFlightService
    {
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "GkKrT~ky.YfOof>?\\uT%Mpl,>2,ged~nMAVyOB&<^`G?2XXpG@_Ad@=k";
        private static readonly Lazy<SynapTronFlightService> lazy = new(() => new SynapTronFlightService());

        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;
        private readonly string _systemInstructions;

        public static SynapTronFlightService Instance => lazy.Value;

        private SynapTronFlightService()
        {
            var enigma = new Enigma3Service();
            _groqApiKey = enigma.Decrypt(ApiKeyGuid, EncryptedApiKey);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

            _systemInstructions =
                "You are SynapTron FlightGenerator, a backend AI that generates realistic flight search results.\n" +
                "Your task:\n" +
                "- Generate 3-5 realistic flight options for the given route and date\n" +
                "- Use appropriate airlines for each route (e.g., Lufthansa/Ryan Air for Europe, Turkish Airlines for Turkey routes)\n" +
                "- Price realistically: Budget airlines 30-50% cheaper, Premium airlines 50-100% more expensive\n" +
                "- Use logical departure/arrival times spread throughout the day\n" +
                "- Flight duration should be realistic for the distance\n" +
                "- NEVER use international carriers (Emirates, Delta, etc.) for domestic routes\n" +
                "- Always use proper IATA airport codes\n\n" +
                "Output ONLY valid JSON in this exact BookingCom API format:\n" +
                "{\n" +
                "  \"data\": {\n" +
                "    \"flightOffers\": [\n" +
                "      {\n" +
                "        \"id\": \"flight_id\",\n" +
                "        \"price\": {\n" +
                "          \"total\": \"150.00\",\n" +
                "          \"currency\": \"EUR\"\n" +
                "        },\n" +
                "        \"itineraries\": [\n" +
                "          {\n" +
                "            \"segments\": [\n" +
                "              {\n" +
                "                \"departure\": {\n" +
                "                  \"iataCode\": \"MUC\",\n" +
                "                  \"at\": \"2025-08-15T08:30:00\"\n" +
                "                },\n" +
                "                \"arrival\": {\n" +
                "                  \"iataCode\": \"LHR\",\n" +
                "                  \"at\": \"2025-08-15T10:15:00\"\n" +
                "                },\n" +
                "                \"carrierCode\": \"LH\",\n" +
                "                \"number\": \"1234\",\n" +
                "                \"aircraft\": \"A320\",\n" +
                "                \"duration\": \"PT1H45M\"\n" +
                "              }\n" +
                "            ]\n" +
                "          }\n" +
                "        ],\n" +
                "        \"validatingAirlineCodes\": [\"LH\"]\n" +
                "      }\n" +
                "    ]\n" +
                "  }\n" +
                "}\n\n" +
                "Generate realistic data. Do not add any explanation or markdown formatting.";
        }

        public async Task<string> GenerateFlightResultsAsync(string fromId, string toId, string departureDate)
        {
            var prompt = $"Generate flight results for: {fromId} to {toId} on {departureDate}";

            var messages = new List<Dictionary<string, string>>
            {
                new() { ["role"] = "system", ["content"] = _systemInstructions },
                new() { ["role"] = "user", ["content"] = prompt }
            };

            try
            {
                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = messages,
                    temperature = 0.7  // Add some randomness for variety
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

                var cleanedJson = completion.Trim();
                if (cleanedJson.StartsWith("```json")) cleanedJson = cleanedJson[7..];
                if (cleanedJson.EndsWith("```")) cleanedJson = cleanedJson[..^3];
                cleanedJson = cleanedJson.Trim();

                return cleanedJson;
            }
            catch (Exception ex)
            {
                // Fallback to basic structure if LLM fails
                return GenerateFallbackFlightResults(fromId, toId, departureDate);
            }
        }

        private string GenerateFallbackFlightResults(string fromId, string toId, string departureDate)
        {
            var fallbackResult = new
            {
                data = new
                {
                    flightOffers = new[]
                    {
                        new
                        {
                            id = "fallback_flight_001",
                            price = new { total = "199.00", currency = "EUR" },
                            itineraries = new[]
                            {
                                new
                                {
                                    segments = new[]
                                    {
                                        new
                                        {
                                            departure = new { iataCode = fromId, at = $"{departureDate}T09:00:00" },
                                            arrival = new { iataCode = toId, at = $"{departureDate}T11:30:00" },
                                            carrierCode = "LH",
                                            number = "2001",
                                            aircraft = "A320",
                                            duration = "PT2H30M"
                                        }
                                    }
                                }
                            },
                            validatingAirlineCodes = new[] { "LH" }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(fallbackResult);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}