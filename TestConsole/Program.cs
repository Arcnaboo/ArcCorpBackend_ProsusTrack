using ArcCorpBackend.Services;
using System;

namespace TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var enigma = new Enigma3Service();
            var id = Guid.NewGuid();
            Console.WriteLine($"Random GUID for this session: {id}");
            Console.WriteLine("Type 'encrypt yourtext' or 'decrypt yourtext' to use Enigma3. Type 'exit' to quit.");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input == null) continue;

                if (input.Trim().ToLower() == "exit")
                    break;

                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid command. Use: encrypt yourtext OR decrypt yourtext");
                    continue;
                }

                var command = parts[0].ToLower();
                var text = parts[1];

                try
                {
                    switch (command)
                    {
                        case "encrypt":
                            var encrypted = enigma.Encrypt(id, text);
                            Console.WriteLine($"Encrypted: {encrypted}");
                            break;
                        case "decrypt":
                            var decrypted = enigma.Decrypt(id, text);
                            Console.WriteLine($"Decrypted: {decrypted}");
                            break;
                        default:
                            Console.WriteLine("Unknown command. Use: encrypt or decrypt");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
