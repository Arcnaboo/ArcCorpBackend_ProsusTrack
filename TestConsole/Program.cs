using ArcCorpBackend.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var enigma = new Enigma3Service();
            var id = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
            Console.WriteLine($"Random GUID for this session: {id}");

            Console.WriteLine("Choose mode: Type '1' for Enigma3 test, '2' for flight search tester, or '3' for SynapTron main app tester:");
            Console.Write("> ");
            var modeInput = Console.ReadLine()?.Trim();
            if (modeInput == "2")
            {
                Console.WriteLine("Flight Search Tester selected!");
                Console.WriteLine("Enter search params: source destination departure_date (e.g., Country:GB City:dubrovnik_hr 10/08/2025)");
                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input)) continue;
                    if (input.Trim().ToLower() == "exit") break;

                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3)
                    {
                        Console.WriteLine("Invalid input. Use format: source destination date_from (e.g., Country:GB City:dubrovnik_hr 10/08/2025)");
                        continue;
                    }

                    var source = parts[0];
                    var destination = parts[1];
                    var dateStr = parts[2];

                    string dateIso;
                    try
                    {
                        var parsedDate = DateTime.ParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        dateIso = parsedDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid date format. Use dd/MM/yyyy (e.g., 10/08/2025)");
                        continue;
                    }

                    try
                    {
                        var flightParams = new SearchFlightParams
                        {
                            Source = source,
                            Destination = destination,
                            OutboundDepartmentDateStart = dateIso,
                            OutboundDepartmentDateEnd = dateIso,
                        };

                        var result = await MultiApiTravelService.Instance.SearchFlightAsyncKiwi(flightParams);

                        var handler = new FlightResultHandlerService();
                        var cards = handler.Handle(result);

                        Console.WriteLine("Flight cards:");
                        if (cards.Count == 0)
                        {
                            Console.WriteLine("No flights found.");
                        }
                        else
                        {
                            var X = 0;
                            foreach (var card in cards)
                            {
                                Console.WriteLine("--------------");
                                Console.WriteLine($"Title: {card.Title}");
                                Console.WriteLine($"Price: {card.Price}");
                                Console.WriteLine($"Location: {card.Location}");
                                Console.WriteLine($"Details: {card.Details}");
                                Console.WriteLine($"Action: {card.Action.Type} → {card.Action.Url}");
                                Console.WriteLine("--------------");
                                X++;
                                if (X == 5)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            else if (modeInput == "3")
            {
                Console.WriteLine("SynapTron Main App Tester selected!");
                Console.WriteLine("Type your prompt naturally (e.g., 'Find flights from London to Dubai on 10 August 2025'). Type 'exit' to quit.");
                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input)) continue;
                    if (input.Trim().ToLower() == "exit") break;

                    try
                    {
                        var synaptron = SynapTronTravelService.Instance;
                        var response = await synaptron.CategorizeIntent(input);

                        if (response.HasCards && response.Cards.Count > 0)
                        {
                            var X = 0;
                            foreach (var card in response.Cards)
                            {
                                Console.WriteLine("--------------");
                                Console.WriteLine($"Title: {card.Title}");
                                Console.WriteLine($"Price: {card.Price}");
                                Console.WriteLine($"Location: {card.Location}");
                                Console.WriteLine($"Details: {card.Details}");
                                //Console.WriteLine($"Action: {card.Action.Type} → {card.Action.Url}");
                                Console.WriteLine("--------------");
                                X++;
                                if (X == 5)
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine(response.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Enigma3 test mode selected!");
                Console.WriteLine("Type 'encrypt yourtext' or 'decrypt yourtext' to use Enigma3. Type 'exit' to quit.");

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (input == null) continue;
                    if (input.Trim().ToLower() == "exit") break;

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
}
