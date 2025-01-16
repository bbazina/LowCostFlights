namespace LowCostFlight.Domain.Models
{
    public class FilterQuery
    {
        public string OriginIataCode { get; set; }
        public string DestinationIataCode { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int NumberOfPassengers { get; set; }
        public Currency Currency { get; set; }
    }
}
