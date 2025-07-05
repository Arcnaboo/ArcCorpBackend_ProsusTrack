using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public sealed class BookingComAPIService : ITravelService
    {
        
       

        private readonly string ApiKey = "";
        private const string ApiHost = "booking-com18.p.rapidapi.com";
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedApiKey = "^ed<\\TeV|7Mk#.H[THd-<VHV-eT<0^uV|.-&k6.duV...\\^\\<d";
        private static readonly Lazy<BookingComAPIService> lazy = new(() => new BookingComAPIService());


        public static BookingComAPIService Instance => lazy.Value;
        private BookingComAPIService() 
        {
            var encryptor = new Enigma3Service();
            ApiKey = encryptor.Decrypt(ApiKeyGuid, EncryptedApiKey);
        }

        public async Task<string> SearchFlightAsyncKiwi(SearchFlightParams flightParams)
        {
            using var client = new HttpClient();

            string url = BuildFlightUrl(flightParams);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", ApiHost);

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private string BuildFlightUrl(SearchFlightParams flightParams)
        {
            string from = flightParams.Source?.Replace("City:", "").ToUpper() ?? "IST";
            string to = flightParams.Destination?.Replace("City:", "").ToUpper() ?? "ESB";

            string date = DateTime.TryParse(flightParams.OutboundDepartmentDateStart, out var dt)
                ? dt.ToString("yyyy-MM-dd")
                : DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd");

            string cabinClass = flightParams.CabinClass?.ToUpper() ?? "ECONOMY";

            return $"https://booking-com18.p.rapidapi.com/flights/search-oneway?fromId={from}&toId={to}&departureDate={date}&cabinClass={cabinClass}&numberOfStops=nonstop_flights";
        }

        // Other interface methods not implemented:
        Task<string> ITravelService.SearchFlightsAsync(string origin, string destination, string departureDate, string returnDate, int adults) => throw new NotImplementedException();
        Task<string> ITravelService.SearchPropertiesAsync(string location, string checkIn, string checkOut, int adults) => throw new NotImplementedException();
        Task<string> ITravelService.SearchToursAsync(string city, string startDate, string endDate) => throw new NotImplementedException();
        Task<string> ITravelService.ShopAvailabilityAsync(string propertyId, string checkIn, string checkOut, int adults) => throw new NotImplementedException();
    }
}
