using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Domain.Interfaces;
using ArcCorpBackend.Domain.Repositories;

namespace ArcCorpBackend.Services
{
    public class SynapTronChatService
    {
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "GkKrT~ky.YfOof>?\\uT%Mpl,>2,ged~nMAVyOB&<^`G?2XXpG@_Ad@=k";

        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;
        private readonly List<Dictionary<string, string>> _globalHistory;
        private readonly string _systemFacts;
        private readonly IUsersRepository _usersRepository = new UsersRepository();
        private readonly User _user;
        public SynapTronChatService(User user, string chatId)
        {
            if (!Guid.TryParse(chatId, out Guid parsedChatId))
                throw new ArgumentException("Invalid chatId format");
            _user = user;
            var enigma = new Enigma3Service();
            _groqApiKey = enigma.Decrypt(ApiKeyGuid, EncryptedApiKey);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

            _globalHistory = new List<Dictionary<string, string>>();

            _systemFacts =
                "You are SynapTron, a backend travel assistant AI specialized ONLY in Flight Booking requests.\n" +
                "This version of SynapTron was built specifically for the Raise 2025 Hackathon Prosus track.\n" +
                "You NEVER greet casually or act like a chatbot. You respond formally and directly.\n\n" +
                "🎯 Your task is:\n" +
                "1. Read the user message in the format {userMessage: \"...\"}.\n" +
                "2. Extract the required fields: fromId, toId, departureDate.\n" +
                "3. If any of these fields are missing, set readyForAction:false, list the missing fields in missingContext, and generate a polite question in userPrompt asking only for the missing details.\n" +
                "4. If all required fields are present, set readyForAction:true and output the finalized travel details as a plain English summary in userPrompt. NEVER leave userPrompt null or empty when readyForAction is true.\n\n" +
                "⚠️ IMPORTANT:\n" +
                "If a user requests anything other than flight bookings (e.g., hotels, tours, cars), respond politely and inform them that this SynapTron version for Raise 2025 Prosus track ONLY handles flight bookings and cannot process other types of requests.\n\n" +
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
            // here we should consider adding user history
            List<UserData> datas = _usersRepository.GetUserDataForUser(user.UserId);

            //here add userdata to globalhistory possibly as additional sys fact
            //List<UserData> datas = _usersRepository.GetUserDataForUser(user.UserId);

            foreach (var data in datas)
            {
                _globalHistory.Add(new Dictionary<string, string>
                {
                    ["role"] = "system",
                    ["content"] = $"User preference: {data.Message}"
                });
            }

            _globalHistory.Add(new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = _systemFacts
            });

            Chat targetChat = null;
            foreach (var chat in user.Chats)
            {
                if (chat.ChatId == parsedChatId)
                {
                    targetChat = chat;
                    break;
                }
            }

            if (targetChat == null)
                throw new KeyNotFoundException($"Chat with ID {chatId} not found for user {user.Email}.");

            foreach (var message in targetChat.Messages)
            {
                _globalHistory.Add(new Dictionary<string, string>
                {
                    ["role"] = message.IsUserMessage ? "user" : "assistant",
                    ["content"] = message.Content
                });
            }
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
}
