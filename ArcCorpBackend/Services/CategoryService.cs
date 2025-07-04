using System.Collections.Generic;
using System.Text;

namespace ArcCorpBackend.Services
{
    public class CategoryService
    {
        private readonly Dictionary<string, List<string>> _categoryContexts = new()
        {
            ["Flight Booking"] = new() { "Departure city", "Destination city", "Departure date", "Return date (optional)", "Passenger count and types", "Cabin class" },
            ["Hotel Booking"] = new() { "Destination", "Check-in date", "Check-out date", "Number of guests", "Room type", "Budget (optional)" },
            ["Train Booking"] = new() { "Departure station", "Arrival station", "Travel date", "Seat class", "Passenger count" },
            ["Bus Booking"] = new() { "Departure location", "Arrival location", "Travel date", "Passenger count" },
            ["Car Rental"] = new() { "Pickup location", "Drop-off location", "Pickup date/time", "Return date/time", "Driver age", "Car type" },
            ["Tour Booking"] = new() { "Location", "Tour type", "Date", "Participant count", "Language preference" },
            ["Vacation Package"] = new() { "Destination", "Travel dates", "Number of travelers", "Budget", "Vacation type" },
            ["Local Experience"] = new() { "Experience type", "Location", "Date", "Guest count" }
        };

        public List<string> GetAllCategories() => new(_categoryContexts.Keys);

        public List<string> GetRequiredContext(string category)
            => _categoryContexts.TryGetValue(category, out var contextList)
                ? contextList
                : new List<string> { "Unknown category" };

        public string RequestContext(string category)
        {
            if (!_categoryContexts.ContainsKey(category))
                return "I'm sorry, I couldn't recognize that category. Could you please rephrase?";

            var fields = _categoryContexts[category];
            var builder = new StringBuilder($"To assist you with **{category}**, could you kindly provide the following information:\n");

            foreach (var field in fields)
                builder.AppendLine($"- {field}");

            return builder.ToString();
        }
    }
}
