using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public interface ITravelService
    {
        Task<string> SearchPropertiesAsync(string location, string checkIn, string checkOut, int adults = 1);

        Task<string> ShopAvailabilityAsync(string propertyId, string checkIn, string checkOut, int adults = 1);

        Task<string> SearchFlightsAsync(string origin, string destination, string departureDate, string returnDate = "", int adults = 1);
        Task<string> SearchFlightAsyncKiwi(SearchFlightParams flightParams);

        Task<string> SearchToursAsync(string city, string startDate, string endDate);
    }
}
