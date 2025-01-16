using LowCostFlight.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LowCostFlight.Repository.Repository
{
    public class FlightRepository : IFlightRepository
    {
        private readonly LowCostFlightDbContext _dbContext;

        public FlightRepository(LowCostFlightDbContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Flight>> GetFlightsAsync(FilterQuery filter)
        {
            var flights = await _dbContext.Set<Flight>()
                    .Where(f =>
                        f.OriginAirport == filter.OriginIataCode &&
                        f.DestinationAirport == filter.DestinationIataCode &&
                        f.DepartureDate.Date == filter.DepartureDate.Date &&
                        (!filter.ReturnDate.HasValue || f.ReturnDate.HasValue && f.ReturnDate.Value.Date == filter.ReturnDate.Value.Date) &&
                        f.NumberOfPassengers == filter.NumberOfPassengers &&
                        f.Currency == filter.Currency)
                    .ToListAsync();

            return flights;
        }

        public async Task AddFlightsAsync(IEnumerable<Flight> flights)
        {
            await _dbContext.Set<Flight>().AddRangeAsync(flights);
            await _dbContext.SaveChangesAsync();
        }
    }
}
