using LowCostFlight.Core.Services;
using LowCostFlight.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LowCostFlight.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFlightsAsync([FromQuery] FilterQuery filterQuery)
        {
            var result = await _flightService.GetFlightsAsync(filterQuery);

            return Ok(result);
        }
    }
}
