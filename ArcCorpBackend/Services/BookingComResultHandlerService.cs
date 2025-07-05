using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Services
{
    public class BookingComResultHandlerService
    {
        public List<Card> Handle(string json)
        {
            var cards = new List<Card>();

            BookingComResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<BookingComResponse>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse Booking.com flight JSON: {ex.Message}", ex);
            }

            var flights = response?.Data?.Flights;
            if (flights == null || flights.Count == 0)
                return cards;

            foreach (var flight in flights)
            {
                if (flight.Bounds == null || flight.Bounds.Count == 0)
                    continue;

                var bound = flight.Bounds[0];
                if (bound.Segments == null || bound.Segments.Count == 0)
                    continue;

                var segment = bound.Segments[0];

                var carrierName = segment.MarketingCarrier?.Name ?? "Unknown Carrier";
                var price = flight.TravelerPrices?.Count > 0
                    ? $"{flight.TravelerPrices[0].Price?.TotalAmount()} USD"
                    : "N/A";

                var dep = segment.Origin?.AirportName ?? "?";
                var arr = segment.Destination?.AirportName ?? "?";

                var depTime = segment.DeparturedAt ?? "?";
                var arrTime = segment.ArrivedAt ?? "?";

                var details = $"From {dep} at {depTime} → To {arr} at {arrTime}";

                var url = flight.ShareableUrl ?? "https://booking.com";

                var card = new Card
                {
                    Title = carrierName,
                    Price = price,
                    Location = $"{dep} → {arr}",
                    Details = details,
                    Action = new ActionObj
                    {
                        Type = "Book Now",
                        Url = url
                    }
                };

                cards.Add(card);
            }

            return cards;
        }
    }

    public class BookingComResponse
    {
        [JsonPropertyName("data")]
        public BookingComData? Data { get; set; }
    }

    public class BookingComData
    {
        [JsonPropertyName("flights")]
        public List<BookingComFlight>? Flights { get; set; }
    }

    public class BookingComFlight
    {
        [JsonPropertyName("bounds")]
        public List<BookingComBound>? Bounds { get; set; }

        [JsonPropertyName("travelerPrices")]
        public List<BookingComTravelerPrice>? TravelerPrices { get; set; }

        [JsonPropertyName("shareableUrl")]
        public string? ShareableUrl { get; set; }
    }

    public class BookingComBound
    {
        [JsonPropertyName("segments")]
        public List<BookingComSegment>? Segments { get; set; }
    }

    public class BookingComSegment
    {
        [JsonPropertyName("departuredAt")]
        public string? DeparturedAt { get; set; }

        [JsonPropertyName("arrivedAt")]
        public string? ArrivedAt { get; set; }

        [JsonPropertyName("origin")]
        public BookingComAirport? Origin { get; set; }

        [JsonPropertyName("destination")]
        public BookingComAirport? Destination { get; set; }

        [JsonPropertyName("marketingCarrier")]
        public BookingComCarrier? MarketingCarrier { get; set; }
    }

    public class BookingComAirport
    {
        [JsonPropertyName("airportName")]
        public string? AirportName { get; set; }
    }

    public class BookingComCarrier
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class BookingComTravelerPrice
    {
        [JsonPropertyName("price")]
        public BookingComPrice? Price { get; set; }
    }

    public class BookingComPrice
    {
        [JsonPropertyName("price")]
        public BookingComAmount? PriceValue { get; set; }

        [JsonPropertyName("vat")]
        public BookingComAmount? Vat { get; set; }

        public string TotalAmount()
        {
            var total = (PriceValue?.Value ?? 0) + (Vat?.Value ?? 0);
            total = total / 100m;
            return total.ToString("F2");
        }
    }

    public class BookingComAmount
    {
        [JsonPropertyName("value")]
        public decimal? Value { get; set; }
    }
}
