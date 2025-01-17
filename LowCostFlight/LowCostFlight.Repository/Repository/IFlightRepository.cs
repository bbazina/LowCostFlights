using LowCostFlight.Domain.Models;

namespace LowCostFlight.Repository.Repository
{
    public interface IFlightRepository
    {
        Task<PaginatedResponse<Flight>> GetFlightsAsync(FilterQuery filter);
        Task AddFlightsAsync(IEnumerable<Flight> flights);
    }
}
