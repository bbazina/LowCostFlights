using LowCostFlight.Domain.Models;

namespace LowCostFlight.Repository.Repository
{
    public interface IFlightRepository
    {
        /// <summary>
        /// Retrieves flights based on the provided filter.
        /// </summary>
        /// <param name="filter">FilterQuery object containing the search criteria.</param>
        /// <returns>A list of flights matching the filter criteria.</returns>
        Task<List<Flight>> GetFlightsAsync(FilterQuery filter);

        /// <summary>
        /// Adds flights in bulk to the database.
        /// </summary>
        /// <param name="flights">List of Flight objects to add.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task AddFlightsAsync(IEnumerable<Flight> flights);
    }
}
