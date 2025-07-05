using System.Text.Json.Serialization;

namespace ArcCorpBackend.Services
{
    public class SearchFlightParams
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("adults")]
        public int? Adults { get; set; } = 1;

        [JsonPropertyName("children")]
        public int? Children { get; set; } = 0;

        [JsonPropertyName("infants")]
        public int? Infants { get; set; } = 0;

        [JsonPropertyName("handbags")]
        public int? Handbags { get; set; } = 1;

        [JsonPropertyName("holdbags")]
        public int? Holdbags { get; set; } = 0;

        [JsonPropertyName("cabinClass")]
        public string? CabinClass { get; set; } = "ECONOMY";

        [JsonPropertyName("sortBy")]
        public string? SortBy { get; set; } = "QUALITY";

        [JsonPropertyName("sortOrder")]
        public string? SortOrder { get; set; } = "ASCENDING";

        [JsonPropertyName("applyMixedClasses")]
        public string? ApplyMixedClasses { get; set; } = "true";

        [JsonPropertyName("allowReturnFromDifferentCity")]
        public string? AllowReturnFromDifferentCity { get; set; } = "true";

        [JsonPropertyName("allowChangeInboundDestination")]
        public string? AllowChangeInboundDestination { get; set; } = "true";

        [JsonPropertyName("allowChangeInboundSource")]
        public string? AllowChangeInboundSource { get; set; } = "true";

        [JsonPropertyName("allowDifferentStationConnection")]
        public string? AllowDifferentStationConnection { get; set; } = "true";

        [JsonPropertyName("enableSelfTransfer")]
        public string? EnableSelfTransfer { get; set; } = "true";

        [JsonPropertyName("allowOvernightStopover")]
        public string? AllowOvernightStopover { get; set; } = "true";

        [JsonPropertyName("enableTrueHiddenCity")]
        public string? EnableTrueHiddenCity { get; set; } = "true";

        [JsonPropertyName("enableThrowAwayTicketing")]
        public string? EnableThrowAwayTicketing { get; set; } = "true";

        [JsonPropertyName("priceStart")]
        public int? PriceStart { get; set; } = 0;

        [JsonPropertyName("priceEnd")]
        public int? PriceEnd { get; set; } = 0;

        [JsonPropertyName("maxStopsCount")]
        public int? MaxStopsCount { get; set; } = 0;

        [JsonPropertyName("outbound")]
        public string? Outbound { get; set; }

        [JsonPropertyName("transportTypes")]
        public string? TransportTypes { get; set; } = "FLIGHT";

        [JsonPropertyName("contentProviders")]
        public string? ContentProviders { get; set; } = "FLIXBUS_DIRECTS,FRESH,KAYAK,KIWI";

        [JsonPropertyName("limit")]
        public int? Limit { get; set; } = 20;

        [JsonPropertyName("inboundDepartureDateStart")]
        public string? InboundDepartureDateStart { get; set; }

        [JsonPropertyName("inboundDepartureDateEnd")]
        public string? InboundDepartureDateEnd { get; set; }

        [JsonPropertyName("outboundDepartmentDateStart")]
        public string? OutboundDepartmentDateStart { get; set; }

        [JsonPropertyName("outboundDepartmentDateEnd")]
        public string? OutboundDepartmentDateEnd { get; set; }
    }
}
