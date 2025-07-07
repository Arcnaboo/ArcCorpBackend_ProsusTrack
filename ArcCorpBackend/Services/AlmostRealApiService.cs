using System;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class AlmostRealApiService : ITravelService
    {
        private readonly SynapTronFlightService _flightService;

        public static AlmostRealApiService Instance = new AlmostRealApiService();

        public AlmostRealApiService()
        {
            _flightService = SynapTronFlightService.Instance;
        }

        public async Task<string> SearchFlightsAsync(string origin, string destination, string departureDate, string returnDate = "", int adults = 1)
        {
            // Call your LLM-powered SynapTron flight generator:
            var result = await _flightService.GenerateFlightResultsAsync(origin, destination, departureDate);
            return result;
        }

        public Task<string> SearchFlightAsyncKiwi(SearchFlightParams flightParams)
        {
            throw new NotImplementedException("Kiwi flight search is not supported in AlmostRealApiService.");
        }

        public Task<string> SearchPropertiesAsync(string location, string checkIn, string checkOut, int adults = 1)
        {
            throw new NotImplementedException("Property search is not supported in AlmostRealApiService.");
        }

        public Task<string> ShopAvailabilityAsync(string propertyId, string checkIn, string checkOut, int adults = 1)
        {
            throw new NotImplementedException("Shop availability is not supported in AlmostRealApiService.");
        }

        public Task<string> SearchToursAsync(string city, string startDate, string endDate)
        {
            throw new NotImplementedException("Tour search is not supported in AlmostRealApiService.");
        }
    }
}
