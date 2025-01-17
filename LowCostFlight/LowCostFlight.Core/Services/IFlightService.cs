using LowCostFlight.Domain.Models;

namespace LowCostFlight.Core.Services
{
    public interface IFlightService
    {
        Task<PaginatedResponse<Flight>> GetFlightsAsync(FilterQuery filter);
    }
}
