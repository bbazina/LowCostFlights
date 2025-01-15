namespace LowCostFlight.Domain.Models
{
    public class Flight
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int DepartureNumberOfStops { get; set; }
        public int ReturnNumberOfStops { get; set; }
        public int NumberOfPassengers { get; set; }
        public Currency Currency { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
