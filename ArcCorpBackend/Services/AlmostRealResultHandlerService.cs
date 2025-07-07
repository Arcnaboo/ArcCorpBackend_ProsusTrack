using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Services
{
    public class AlmostRealResultHandlerService
    {
        public List<Card> Handle(string json)
        {
            var cards = new List<Card>();

            SynapTronFlightResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<SynapTronFlightResponse>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse SynapTron flight JSON: {ex.Message}", ex);
            }

            var offers = response?.Data?.FlightOffers;
            if (offers == null || offers.Count == 0)
                return cards;

            foreach (var offer in offers)
            {
                if (offer.Itineraries == null || offer.Itineraries.Count == 0)
                    continue;

                var segment = offer.Itineraries[0].Segments?[0];
                if (segment == null)
                    continue;

                var details = $"Departs {segment.Departure?.IataCode} at {segment.Departure?.At} → Arrives {segment.Arrival?.IataCode} at {segment.Arrival?.At}";
                var price = $"{offer.Price?.Total} {offer.Price?.Currency}";

                var card = new Card
                {
                    Title = $"{segment.CarrierCode} Flight {segment.Number}",
                    Price = price,
                    Location = $"{segment.Departure?.IataCode} → {segment.Arrival?.IataCode}",
                    Details = details,
                    Action = new ActionObj
                    {
                        Type = "Book Now",
                        Url = "https://arc-flight-booking" // Placeholder booking URL
                    }
                };

                cards.Add(card);
            }

            return cards;
        }
    }

    public class SynapTronFlightResponse
    {
        [JsonPropertyName("data")]
        public SynapTronFlightData? Data { get; set; }
    }

    public class SynapTronFlightData
    {
        [JsonPropertyName("flightOffers")]
        public List<SynapTronFlightOffer>? FlightOffers { get; set; }
    }

    public class SynapTronFlightOffer
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("price")]
        public SynapTronPrice? Price { get; set; }

        [JsonPropertyName("itineraries")]
        public List<SynapTronItinerary>? Itineraries { get; set; }
    }

    public class SynapTronPrice
    {
        [JsonPropertyName("total")]
        public string? Total { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    public class SynapTronItinerary
    {
        [JsonPropertyName("segments")]
        public List<SynapTronSegment>? Segments { get; set; }
    }

    public class SynapTronSegment
    {
        [JsonPropertyName("departure")]
        public SynapTronLocation? Departure { get; set; }

        [JsonPropertyName("arrival")]
        public SynapTronLocation? Arrival { get; set; }

        [JsonPropertyName("carrierCode")]
        public string? CarrierCode { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("aircraft")]
        public string? Aircraft { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }
    }

    public class SynapTronLocation
    {
        [JsonPropertyName("iataCode")]
        public string? IataCode { get; set; }

        [JsonPropertyName("at")]
        public string? At { get; set; }
    }
}
