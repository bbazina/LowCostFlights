using LowCostFlight.Core.Helper;
using LowCostFlight.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LowCostFlight.Core.Services
{
    public class FlightService : IFlightService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public FlightService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<List<Flight>> GetFlightsAsync(FilterQuery filter)
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            var token = await _tokenService.GetAccessTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = GetUrlWithParams(token, filter);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) 
            {
                throw new Exception("Failed to fetch flight data");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var flightData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (!flightData.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("No flight data found in the response.");
            }

            var flights = new List<Flight>();
            foreach (var flightItem in dataElement.EnumerateArray())
            {
                var flight = new Flight();

                // Mapping Origin and Destination Airports
                var itineraries = flightItem.GetProperty("itineraries").EnumerateArray();
                var firstItinerary = itineraries.First();
                var lastItinerary = itineraries.Last();

                var firstSegment = firstItinerary.GetProperty("segments").EnumerateArray().First();
                var lastSegment = lastItinerary.GetProperty("segments").EnumerateArray().Last();

                // Origin is the first departure segment
                flight.OriginAirport = firstSegment.GetProperty("departure").GetProperty("iataCode").GetString();

                // Destination is the last arrival segment
                flight.DestinationAirport = lastSegment.GetProperty("arrival").GetProperty("iataCode").GetString();


                // Mapping Dates
                flight.DepartureDate = DateTime.Parse(firstSegment.GetProperty("departure").GetProperty("at").GetString());
                flight.ReturnDate = DateTime.Parse(lastSegment.GetProperty("arrival").GetProperty("at").GetString());

                // Mapping Stop Counts
                flight.DepartureNumberOfStops = firstSegment.GetProperty("numberOfStops").GetInt32();
                flight.ReturnNumberOfStops = lastSegment.GetProperty("numberOfStops").GetInt32();

                // Mapping Passengers and Currency
                flight.NumberOfPassengers = filter.NumberOfPassengers;
                flight.Currency = Enum.TryParse(flightItem.GetProperty("price").GetProperty("currency").GetString(), out Currency currency)
                                  ? currency : Currency.USD;

                flight.TotalPrice = decimal.TryParse(flightItem.GetProperty("price").GetProperty("total").GetString(), out var total)
                                    ? total : 0m;

                // Add the flight to the list
                flights.Add(flight);
            }

            return flights;
        }

        private string GetUrlWithParams(string token, FilterQuery filter)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var apiUrl = _configuration.GetSection("AmadeusApi:ApiUrl").Value;
                var queryParams = new Dictionary<string, string>()
                {
                    { "originLocationCode", filter.OriginIataCode },
                    { "destinationLocationCode", filter.DestinationIataCode },
                    { "departureDate", filter.DepartureDate.ToString("yyyy-MM-dd") },
                    { "returnDate", filter.ReturnDate.ToString("yyyy-MM-dd") },
                    { "adults", filter.NumberOfPassengers.ToString() },
                    { "currencyCode", filter.Currency.GetCurrency() }
                };

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var fullUrl = $"{apiUrl}?{queryString}";
                return fullUrl;
            }

            return string.Empty;
        }
    }
}
