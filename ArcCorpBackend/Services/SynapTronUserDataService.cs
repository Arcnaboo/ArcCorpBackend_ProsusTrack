using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class SynapTronUserDataService
    {
        private static SynapTronUserDataService? _instance = null;
        private readonly HttpClient _httpClient;
        private readonly List<Dictionary<string, string>> _globalHistory;
        private readonly string _systemInstructions;
        private readonly string _groqApiKey = "YOUR_GROQ_API_KEY";
        private readonly IUsersRepository _usersRepository;

        private SynapTronUserDataService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");

            _systemInstructions =
                "You are SynapTron UserDataEvaluator, a backend AI deciding if a user prompt should be stored in UserData history.\n" +
                "Your task:\n" +
                "- Determine if the user's prompt contains important user preferences or context worth saving for future personalization.\n" +
                "- If the prompt contains meaningful, persistent information (e.g., travel dates, destinations, budget), answer with 'Yay - [data to save]'.\n" +
                "- If it does not contain such info (e.g., small talk, unrelated questions), answer with 'Nay'.\n\n" +
                "Only answer Yay or Nay as described above. Provide no other explanation.";

            _globalHistory = new List<Dictionary<string, string>>
            {
                new() { ["role"] = "system", ["content"] = _systemInstructions }
            };
        }

        public static SynapTronUserDataService Create(IUsersRepository usersRepository)
        {
            _instance ??= new SynapTronUserDataService(usersRepository);
            return _instance;
        }

        public async Task<string> EvaluatePromptAsync(string userPrompt, User user)
        {
            _globalHistory.Add(new() { ["role"] = "user", ["content"] = userPrompt });

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = _globalHistory
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

            var trimmed = completion.Trim();
            if (trimmed.StartsWith("Yay", StringComparison.OrdinalIgnoreCase))
            {
                var split = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    var extractedData = split[1].Trim();
                    var userData = new UserData(user, extractedData);
                    await _usersRepository.AddUserDataAsync(userData);
                    await _usersRepository.SaveChangesAsync();
                }
            }

            return trimmed;
        }
    }
}
