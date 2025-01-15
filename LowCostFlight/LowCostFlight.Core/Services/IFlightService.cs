using LowCostFlight.Domain.Models;

namespace LowCostFlight.Core.Services
{
    public interface IFlightService
    {
        Task<List<Flight>> GetFlightsAsync(FilterQuery filter);
    }
}
