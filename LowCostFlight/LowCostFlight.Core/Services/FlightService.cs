using LowCostFlight.Core.Helper;
using LowCostFlight.Domain.Models;
using LowCostFlight.Repository.Repository;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IFlightRepository _flightRepository;
        private readonly IDistributedCache _distributedCache;

        public FlightService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IConfiguration configuration,
            IFlightRepository flightRepository,
            IDistributedCache distributedCache)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _configuration = configuration;
            _flightRepository = flightRepository;
            _distributedCache = distributedCache;
        }

        public async Task<PaginatedResponse<Flight>> GetFlightsAsync(FilterQuery filter)
        {
            // Step 1: Try to fetch data from cache
            var cachedResponse = await GetFlightsFromCacheAsync(filter);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            // Step 2: Try to fetch data from the database
            var resultFromDb = await _flightRepository.GetFlightsAsync(filter);
            if (resultFromDb.Items.Count > 0)
            {
                // Cache the paginated result
                await SetFlightsToCacheAsync(filter, resultFromDb);
                return resultFromDb;
            }

            // Step 3: Fetch flights from external API if not found in DB
            var flightsFromApi = await GetFlightsFromApiAsync(filter);
            if (flightsFromApi.Count > 0)
            {
                // Save flights to the database
                await _flightRepository.AddFlightsAsync(flightsFromApi);

                // Fetch the newly added data as paginated response
                var newResultFromDb = await _flightRepository.GetFlightsAsync(filter);
                await SetFlightsToCacheAsync(filter, newResultFromDb);
                return newResultFromDb;
            }

            // Step 4: Return an empty paginated response if no data is found
            return new PaginatedResponse<Flight>
            {
                Items = new List<Flight>(),
                TotalCount = 0,
                PageCount = 0,
            };
        }

        private async Task<PaginatedResponse<Flight>?> GetFlightsFromCacheAsync(FilterQuery filter)
        {
            string cacheKey = GenerateCacheKey(filter);
            var cachedFlights = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedFlights))
            {
                return JsonSerializer.Deserialize<PaginatedResponse<Flight>>(cachedFlights);
            }

            return null;
        }

        private async Task SetFlightsToCacheAsync(FilterQuery filter, PaginatedResponse<Flight> paginatedResponse)
        {
            string cacheKey = GenerateCacheKey(filter);
            var flightsJson = JsonSerializer.Serialize(paginatedResponse);

            await _distributedCache.SetStringAsync(cacheKey, flightsJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }

        private async Task<List<Flight>> GetFlightsFromApiAsync(FilterQuery filter)
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            var token = await _tokenService.GetAccessTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = GetUrlWithParams(token, filter);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch flight data from API");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var flightData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            if (!flightData.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("No flight data found in the response from the API.");
            }

            return MapFlights(dataElement, filter);
        }

        private string GenerateCacheKey(FilterQuery filter)
        {
            return $"flights_{filter.OriginIataCode}_{filter.DestinationIataCode}_{filter.DepartureDate:yyyyMMdd}_{filter.ReturnDate:yyyyMMdd}_{filter.NumberOfPassengers}_{filter.Currency}_page_{filter.Page}";
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
                    { "returnDate", filter.ReturnDate.HasValue ? filter.ReturnDate.Value.ToString("yyyy-MM-dd") : null },
                    { "adults", filter.NumberOfPassengers.ToString() },
                    { "currencyCode", filter.Currency.GetCurrency() },
                };

                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var fullUrl = $"{apiUrl}?{queryString}";
                return fullUrl;
            }

            return string.Empty;
        }

        private static List<Flight> MapFlights(JsonElement dataElement, FilterQuery filter)
        {
            var flights = new List<Flight>();

            foreach (var flightItem in dataElement.EnumerateArray())
            {
                var flight = new Flight();

                var itineraries = flightItem.GetProperty("itineraries").EnumerateArray();
                var firstItinerary = itineraries.First();
                var lastItinerary = itineraries.Last();

                // Get the first and last segments of the itineraries
                var firstSegment = firstItinerary.GetProperty("segments").EnumerateArray().First();
                var lastSegment = lastItinerary.GetProperty("segments").EnumerateArray().Last();

                // Set the origin airport from the first segment's departure (first itinerary)
                flight.OriginAirport = firstSegment.GetProperty("departure").GetProperty("iataCode").GetString();

                // Set the destination airport based on whether it's a one-way or round-trip flight
                if (itineraries.Count() == 1)
                {
                    // One-way flight: destination is the arrival of the last segment in the first itinerary
                    flight.DestinationAirport = firstItinerary.GetProperty("segments")
                                                               .EnumerateArray()
                                                               .Last()
                                                               .GetProperty("arrival")
                                                               .GetProperty("iataCode")
                                                               .GetString();
                }
                else
                {
                    // Round-trip flight: destination is the arrival of the last segment in the second itinerary (return leg)
                    flight.DestinationAirport = lastItinerary.GetProperty("segments")
                                                              .EnumerateArray()
                                                              .First()
                                                              .GetProperty("departure")
                                                              .GetProperty("iataCode")
                                                              .GetString();
                }

                // Mapping Dates
                flight.DepartureDate = DateTime.Parse(firstSegment.GetProperty("departure").GetProperty("at").GetString());
                flight.ReturnDate = lastSegment.GetProperty("arrival").GetProperty("at").GetString() != null
                    ? DateTime.Parse(lastSegment.GetProperty("arrival").GetProperty("at").GetString())
                    : null;

                // Mapping Stop Counts
                flight.DepartureNumberOfStops = firstSegment.GetProperty("numberOfStops").GetInt32();
                flight.ReturnNumberOfStops = lastSegment.GetProperty("numberOfStops").GetInt32();

                // Mapping Passengers and Currency
                flight.NumberOfPassengers = filter.NumberOfPassengers;
                flight.Currency = Enum.TryParse(flightItem.GetProperty("price").GetProperty("currency").GetString(), out Currency currency)
                                  ? currency : Currency.USD;

                flight.TotalPrice = decimal.TryParse(flightItem.GetProperty("price").GetProperty("total").GetString(), out var total)
                                    ? total : 0m;

                flights.Add(flight);
            }

            return flights;
        }
    }
}
