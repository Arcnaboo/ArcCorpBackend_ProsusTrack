using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Services
{
    public class FlightResultHandlerService
    {
        public List<Card> Handle(string json)
        {
            var cards = new List<Card>();

            KiwiResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<KiwiResponse>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse flight JSON: {ex.Message}", ex);
            }

            var itineraries = response?.Itineraries;
            if (itineraries == null || itineraries.Count == 0)
                return cards;

            foreach (var itinerary in itineraries)
            {
                if (itinerary.Price == null || itinerary.Outbound == null)
                    continue;

                var firstSegment = itinerary.Outbound.SectorSegments?.Count > 0
                    ? itinerary.Outbound.SectorSegments[0].Segment
                    : null;
                if (firstSegment == null) continue;

                var title = firstSegment.Carrier?.Name ?? "Unknown Carrier";
                var priceStr = itinerary.Price.Amount.HasValue
                    ? $"{itinerary.Price.Amount.Value} EUR"
                    : "N/A";

                var details = "";
                if (firstSegment.Source != null && firstSegment.Destination != null)
                {
                    details = $"From {firstSegment.Source.Station.City?.Name} at {firstSegment.Source.LocalTime} → " +
                              $"To {firstSegment.Destination.Station.City?.Name} at {firstSegment.Destination.LocalTime}";
                }

                var bookingUrl = itinerary.BookingOptions?.Edges?.Count > 0
                    ? "https://kiwi.com" + itinerary.BookingOptions.Edges[0].Node?.BookingUrl
                    : "https://kiwi.com";

                var card = new Card
                {
                    Title = title,
                    Price = priceStr,
                    Location = $"{firstSegment.Source?.Station.City?.Name ?? "?"} → {firstSegment.Destination?.Station.City?.Name ?? "?"}",
                    Details = details,
                    Action = new ActionObj
                    {
                        Type = "Book Now",
                        Url = bookingUrl
                    }
                };

                cards.Add(card);
            }

            return cards;
        }
    }

    public class KiwiResponse
    {
        [JsonPropertyName("itineraries")]
        public List<Itinerary>? Itineraries { get; set; }
    }

    public class Itinerary
    {
        [JsonPropertyName("price")]
        public Price? Price { get; set; }

        [JsonPropertyName("bookingOptions")]
        public BookingOptions? BookingOptions { get; set; }

        [JsonPropertyName("outbound")]
        public Sector? Outbound { get; set; }
    }

    public class BookingOptions
    {
        [JsonPropertyName("edges")]
        public List<BookingEdge> Edges { get; set; } = new();
    }

    public class BookingEdge
    {
        [JsonPropertyName("node")]
        public BookingNode? Node { get; set; }
    }

    public class BookingNode
    {
        [JsonPropertyName("bookingUrl")]
        public string? BookingUrl { get; set; }
    }

    public class Sector
    {
        [JsonPropertyName("sectorSegments")]
        public List<SectorSegment> SectorSegments { get; set; } = new();
    }

    public class SectorSegment
    {
        [JsonPropertyName("segment")]
        public Segment? Segment { get; set; }
    }

    public class Segment
    {
        [JsonPropertyName("carrier")]
        public Carrier? Carrier { get; set; }

        [JsonPropertyName("source")]
        public FlightPoint? Source { get; set; }

        [JsonPropertyName("destination")]
        public FlightPoint? Destination { get; set; }
    }

    public class Carrier
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class FlightPoint
    {
        [JsonPropertyName("localTime")]
        public string? LocalTime { get; set; }

        [JsonPropertyName("station")]
        public Station? Station { get; set; }
    }

    public class Station
    {
        [JsonPropertyName("city")]
        public City? City { get; set; }
    }

    public class City
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Price
    {
        [JsonPropertyName("amount")]
        [JsonConverter(typeof(DecimalFromStringConverter))]
        public decimal? Amount { get; set; }
    }

    public class DecimalFromStringConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (decimal.TryParse(str, out var value))
                    return value;
                throw new JsonException($"Invalid decimal string: '{str}'");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
