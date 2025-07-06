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

                // Handle NEW user creation
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

                // Remove Bearer prefix if present
                var tokenInput = input.Trim();
                if (tokenInput.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    tokenInput = tokenInput.Substring("Bearer ".Length).Trim();

                Console.WriteLine("\n[INFO] Token received. Validating...");

                if (AuthService.ValidateToken(tokenInput, out var userId))
                {
                    Console.WriteLine($"[SUCCESS] Token is valid.");
                    Console.WriteLine($"Extracted User ID (sub): {userId}");
                    //Console.WriteLine($"Extracted Token ID (jti): {tokenId}");

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

        static async Task Main(string[] args)
        {
            var key = new byte[64]; // 512 bits
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            Console.WriteLine(Convert.ToHexString(key).ToLower());

            var enigma = new Enigma3Service();
            var id = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
            Console.WriteLine($"Random GUID for this session: {id}");

            Console.WriteLine("Choose mode: Type '1' for Enigma3 test, '2' for flight search tester, '3' for SynapTron main app tester, or '4' for JWT test:");
            Console.Write("> ");
            var modeInput = Console.ReadLine()?.Trim();
            if (modeInput == "2")
            {
                Console.WriteLine("Flight Search Tester selected!");
                // your existing flight search loop...
            }
            else if (modeInput == "3")
            {
                Console.WriteLine("SynapTron Main App Tester selected!");
                // your existing SynapTron loop...
            }
            else if (modeInput == "4")
            {
                await TestJWT();
            }
            else
            {
                Console.WriteLine("Enigma3 test mode selected!");
                // your existing Enigma3 loop...
            }
        }
    }
}
