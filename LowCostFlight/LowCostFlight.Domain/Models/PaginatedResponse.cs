namespace LowCostFlight.Domain.Models
{
    public class PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; }

        // Convenience method to split a list into paginated response
        public static PaginatedResponse<T> FromList(List<T> items, int totalCount, int page, int pageSize)
        {
            int pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PaginatedResponse<T>
            {
                TotalCount = totalCount,
                PageCount = pageCount,
                CurrentPage = page,
                PageSize = pageSize,
                Items = items
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
            };
        }
    }
}
