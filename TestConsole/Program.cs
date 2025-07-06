using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Services;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Program
    {
        static async Task TestJWT()
        {
            Console.WriteLine("JWT Test mode selected!");
            Console.WriteLine("Paste Bearer token, or type 'NEW email@example.com' to create new user, or type 'exit' to quit.\n");

            while (true)
            {
                Console.Write("JWT> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (input.Trim().ToLower() == "exit")
                    break;

                if (input.Trim().StartsWith("NEW ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        Console.WriteLine("[ERROR] Invalid NEW command. Usage: NEW email@example.com");
                        continue;
                    }

                    var email = parts[1].Trim();
                    var user = new User(email);
                    var token = AuthService.GenerateToken(user.UserId.ToString());

                    Console.WriteLine($"[NEW USER CREATED]");
                    Console.WriteLine($"User ID (GUID): {user.UserId}");
                    Console.WriteLine($"Email: {user.Email}");
                    Console.WriteLine($"JWT Token: {token}");
                    Console.WriteLine();
                    continue;
                }

                var tokenInput = input.Trim();
                if (tokenInput.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    tokenInput = tokenInput.Substring("Bearer ".Length).Trim();

                Console.WriteLine("\n[INFO] Token received. Validating...");

                if (AuthService.ValidateToken(tokenInput, out var userId))
                {
                    Console.WriteLine($"[SUCCESS] Token is valid.");
                    Console.WriteLine($"Extracted User ID (sub): {userId}");

                    if (Guid.TryParse(userId, out var guid))
                        Console.WriteLine($"[GUID OK] User ID is a valid GUID: {guid}");
                    else
                        Console.WriteLine("[ERROR] Extracted userId is not a valid GUID!");
                }
                else
                {
                    Console.WriteLine("[FAILURE] Token is invalid or expired.");
                }

                Console.WriteLine();
            }
        }

        static async Task TestChat()
        {
            Console.WriteLine("Chat Tester mode selected!");
            Console.WriteLine("Simulate sending prompts to a chat session. Type 'exit' to quit.\n");

            Console.Write("Enter user email: ");
            var email = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("[ERROR] Email is required.");
                return;
            }

            var user = new User(email);
            Console.WriteLine($"Created user with ID: {user.UserId}");

            var chat = new Chat(user);
            user.Chats.Add(chat);
            ChatService.InitiateChat(user, chat.ChatId.ToString());

            Console.WriteLine($"New chat started with Chat ID: {chat.ChatId}");
            Console.WriteLine("Type your prompts below:\n");

            while (true)
            {
                Console.Write("Chat> ");
                var prompt = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(prompt))
                    continue;
                if (prompt.Trim().ToLower() == "exit")
                    break;

                var response = await ChatService.Query(chat.ChatId.ToString(), prompt);

                Console.WriteLine($"[Response]");
                Console.WriteLine($"Success: {response.Success}");
                Console.WriteLine($"Category: {response.Category}");
                Console.WriteLine($"Message: {response.Message}");
                Console.WriteLine($"ReadyForAction: {response.ReadyForAction}");
                if (response.HasCards && response.Cards.Count > 0)
                {
                    Console.WriteLine($"Cards:");
                    var i = 0;
                    foreach (var card in response.Cards)
                    {
                        i++;
                        Console.WriteLine($"  - Title: {card.Title}");
                        Console.WriteLine($"    Details: {card.Details}");
                        Console.WriteLine($"    Price: {card.Price}");
                        Console.WriteLine($"    Location: {card.Location}");
                        //Console.WriteLine($"    Action: {card.Action.Type} → {card.Action.Url}");
                        if (i == 6) break;
                    }
                }
                else
                {
                    Console.WriteLine($"No cards returned.");
                }
                Console.WriteLine();
            }
        }

        static async Task Main(string[] args)
        {
            var key = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            Console.WriteLine(Convert.ToHexString(key).ToLower());

            var id = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
            Console.WriteLine($"Random GUID for this session: {id}");

            Console.WriteLine("Choose mode: Type '1' for Enigma3 test, '2' for flight search tester, '3' for SynapTron main app tester, '4' for JWT test, or '5' for Chat tester:");
            Console.Write("> ");
            var modeInput = Console.ReadLine()?.Trim();
            if (modeInput == "2")
            {
                Console.WriteLine("Flight Search Tester selected!");
            }
            else if (modeInput == "3")
            {
                Console.WriteLine("SynapTron Main App Tester selected!");
            }
            else if (modeInput == "4")
            {
                await TestJWT();
            }
            else if (modeInput == "5")
            {
                await TestChat();
            }
            else
            {
                Console.WriteLine("Enigma3 test mode selected!");
            }
        }
    }
}
