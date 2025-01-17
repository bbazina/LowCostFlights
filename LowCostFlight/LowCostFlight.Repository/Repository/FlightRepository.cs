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

        public async Task<PaginatedResponse<Flight>> GetFlightsAsync(FilterQuery filter)
        {
            var skip = (filter.Page - 1) * filter.PageSize;
            var take = filter.PageSize;

            var flightQuery = _dbContext.Set<Flight>()
                .AsNoTracking()
                .Where(f =>
                    f.OriginAirport == filter.OriginIataCode &&
                    f.DestinationAirport == filter.DestinationIataCode &&
                    f.DepartureDate.Date == filter.DepartureDate.Date &&
                    (!filter.ReturnDate.HasValue || f.ReturnDate.HasValue && f.ReturnDate.Value.Date == filter.ReturnDate.Value.Date) &&
                    f.NumberOfPassengers == filter.NumberOfPassengers &&
                    f.Currency == filter.Currency);

            var totalCount = await flightQuery.CountAsync();

            var flights = await flightQuery
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync();

            var response = new PaginatedResponse<Flight>
            {
                Items = flights,
                TotalCount = totalCount,
                PageCount = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            };

            return response;
        }

        public async Task AddFlightsAsync(IEnumerable<Flight> flights)
        {
            await _dbContext.Set<Flight>().AddRangeAsync(flights);
            await _dbContext.SaveChangesAsync();
        }
    }
}
