using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    
    public sealed class MultiApiTravelService : ITravelService
    {
        private static readonly Lazy<MultiApiTravelService> lazy = new(() => new MultiApiTravelService());
        public static MultiApiTravelService Instance => lazy.Value;

        private readonly HttpClient _httpKiwi;
        private readonly string _kiwiApiKey = "YOUR_RAPIDAPI_KEY"; // will be overwritten by decrypted key below
        private const string KiwiHost = "kiwi-com-cheap-flights.p.rapidapi.com";
        private const string KiwiBaseUrl = "https://kiwi-com-cheap-flights.p.rapidapi.com";
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "^ed<\\TeV|7Mk#.H[THd-<VHV-eT<0^uV|.-&k6.duV...\\^\\<d";

        private MultiApiTravelService()
        {
            var e3 = new Enigma3Service();
            _kiwiApiKey = e3.Decrypt(ApiKeyGuid, EncryptedApiKey);
            _httpKiwi = new HttpClient
            {
                BaseAddress = new Uri(KiwiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpKiwi.DefaultRequestHeaders.Add("x-rapidapi-key", _kiwiApiKey);
            _httpKiwi.DefaultRequestHeaders.Add("x-rapidapi-host", KiwiHost);
            _httpKiwi.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> SearchFlightsAsync(string origin, string destination, string departureDate, string returnDate = "", int adults = 1)
        {
            // NOTE: Uses the full tested example URL, inserting dynamic origin, destination, departureDate.
            string url = $"/round-trip?source={origin}&destination={destination}" +
                         $"&currency=usd&locale=en&adults={adults}&children=0&infants=0&handbags=1&holdbags=0" +
                         "&cabinClass=ECONOMY&sortBy=QUALITY&sortOrder=ASCENDING&applyMixedClasses=true" +
                         "&allowReturnFromDifferentCity=true&allowChangeInboundDestination=true&allowChangeInboundSource=true" +
                         "&allowDifferentStationConnection=true&enableSelfTransfer=true&allowOvernightStopover=true" +
                         "&enableTrueHiddenCity=true&enableThrowAwayTicketing=true" +
                         "&outbound=SUNDAY%2CWEDNESDAY%2CTHURSDAY%2CFRIDAY%2CSATURDAY%2CMONDAY%2CTUESDAY" +
                         "&transportTypes=FLIGHT&contentProviders=FLIXBUS_DIRECTS%2CFRESH%2CKAYAK%2CKIWI&limit=20" +
                         $"&date_from={departureDate}";

            using var response = await _httpKiwi.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"SearchFlightsAsync (Kiwi RapidAPI) failed {response.StatusCode}: {body}");
            return body;
        }

        // Stubbed methods since your focus is flights only:
        public Task<string> SearchPropertiesAsync(string location, string checkIn, string checkOut, int adults = 1) =>
            throw new NotImplementedException("Hotel search not implemented in MultiApiTravelService");

        public Task<string> ShopAvailabilityAsync(string propertyId, string checkIn, string checkOut, int adults = 1) =>
            throw new NotImplementedException("Availability search not implemented in MultiApiTravelService");

        public Task<string> SearchToursAsync(string city, string startDate, string endDate) =>
            throw new NotImplementedException("Tours search not implemented in MultiApiTravelService");

        public async Task<string> SearchFlightAsyncKiwi(SearchFlightParams flightParams)
        {
            if (string.IsNullOrEmpty(flightParams.Source))
                throw new ArgumentException("Source must be provided");
            if (string.IsNullOrEmpty(flightParams.Destination))
                throw new ArgumentException("Destination must be provided");

            // Build query string using only essential/default params.
            var query = $"source={Uri.EscapeDataString(flightParams.Source)}" +
                        $"&destination={Uri.EscapeDataString(flightParams.Destination)}" +
                        $"&currency={Uri.EscapeDataString(flightParams.Currency ?? "usd")}" +
                        $"&locale={Uri.EscapeDataString(flightParams.Locale ?? "en")}" +
                        $"&adults={flightParams.Adults ?? 1}" +
                        $"&children={flightParams.Children ?? 0}" +
                        $"&infants={flightParams.Infants ?? 0}" +
                        $"&handbags={flightParams.Handbags ?? 1}" +
                        $"&holdbags={flightParams.Holdbags ?? 0}" +
                        $"&cabinClass={Uri.EscapeDataString(flightParams.CabinClass ?? "ECONOMY")}" +
                        $"&sortBy={Uri.EscapeDataString(flightParams.SortBy ?? "QUALITY")}" +
                        $"&sortOrder={Uri.EscapeDataString(flightParams.SortOrder ?? "ASCENDING")}" +
                        $"&applyMixedClasses={flightParams.ApplyMixedClasses ?? "true"}";

            // Optional advanced params included if present:
            if (!string.IsNullOrEmpty(flightParams.Outbound))
                query += $"&outbound={Uri.EscapeDataString(flightParams.Outbound)}";
            if (!string.IsNullOrEmpty(flightParams.TransportTypes))
                query += $"&transportTypes={Uri.EscapeDataString(flightParams.TransportTypes)}";
            if (!string.IsNullOrEmpty(flightParams.ContentProviders))
                query += $"&contentProviders={Uri.EscapeDataString(flightParams.ContentProviders)}";
            if (flightParams.Limit.HasValue)
                query += $"&limit={flightParams.Limit}";

            // Dates (if provided):
            if (!string.IsNullOrEmpty(flightParams.OutboundDepartmentDateStart))
                query += $"&outboundDepartmentDateStart={Uri.EscapeDataString(flightParams.OutboundDepartmentDateStart)}";
            if (!string.IsNullOrEmpty(flightParams.OutboundDepartmentDateEnd))
                query += $"&outboundDepartmentDateEnd={Uri.EscapeDataString(flightParams.OutboundDepartmentDateEnd)}";
            if (!string.IsNullOrEmpty(flightParams.InboundDepartureDateStart))
                query += $"&inboundDepartureDateStart={Uri.EscapeDataString(flightParams.InboundDepartureDateStart)}";
            if (!string.IsNullOrEmpty(flightParams.InboundDepartureDateEnd))
                query += $"&inboundDepartureDateEnd={Uri.EscapeDataString(flightParams.InboundDepartureDateEnd)}";

            // Compose final endpoint
            string url = $"/round-trip?{query}";

            using var response = await _httpKiwi.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"SearchFlightAsyncKiwi failed with status {response.StatusCode}: {body}");
            return body;
        }

    }
}
