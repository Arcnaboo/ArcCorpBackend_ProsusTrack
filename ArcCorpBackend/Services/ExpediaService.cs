using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public sealed class ExpediaService : ITravelService
    {
        private static readonly Lazy<ExpediaService> lazy = new(() => new ExpediaService());
        public static ExpediaService Instance => lazy.Value;

        private readonly HttpClient _http;
        private readonly string _apiKey = "YOUR_EXPEDIA_API_KEY";
        private readonly string _sharedSecret = "YOUR_SHARED_SECRET";
        private const string BaseUrl = "https://test.ean.com/v3"; // swap to production URL when live

        private ExpediaService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private string CreateAuthHeader()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var hash = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(_apiKey + _sharedSecret + timestamp));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return $"EAN APIKey={_apiKey},Signature={signature},timestamp={timestamp}";
        }

        public async Task<string> SearchPropertiesAsync(string location, string checkIn, string checkOut, int adults = 1)
        {
            var path = $"/properties/list?location={Uri.EscapeDataString(location)}&checkIn={checkIn}&checkOut={checkOut}&adults={adults}";
            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(CreateAuthHeader());
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException($"Property Search error {res.StatusCode}: {body}");
            return body;
        }

        public async Task<string> ShopAvailabilityAsync(string propertyId, string checkIn, string checkOut, int adults = 1)
        {
            var path = $"/properties/shop?propertyId={Uri.EscapeDataString(propertyId)}&checkIn={checkIn}&checkOut={checkOut}&adults={adults}";
            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(CreateAuthHeader());
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException($"Shop Availability error {res.StatusCode}: {body}");
            return body;
        }

        public async Task<string> SearchFlightsAsync(string origin, string destination, string departureDate, string returnDate = "", int adults = 1)
        {
            var path = $"/flights/search?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}" +
                       $"&departureDate={departureDate}&adults={adults}";
            if (!string.IsNullOrEmpty(returnDate))
                path += $"&returnDate={returnDate}";

            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(CreateAuthHeader());
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException($"Flight Search error {res.StatusCode}: {body}");
            return body;
        }

        public async Task<string> SearchToursAsync(string city, string startDate, string endDate)
        {
            var path = $"/tours/search?city={Uri.EscapeDataString(city)}&startDate={startDate}&endDate={endDate}";
            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(CreateAuthHeader());
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException($"Tour Search error {res.StatusCode}: {body}");
            return body;
        }

        Task<string> ITravelService.SearchFlightAsyncKiwi(SearchFlightParams flightParams)
        {
            throw new NotImplementedException();
        }
    }
}
