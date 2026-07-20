namespace ESAM.GrowTracking.API.Controllers
{
    public record PagedHttpResponse<TItem>
    {
        public IReadOnlyCollection<TItem> Items { get; init; }

        public int TotalCount { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalPages { get; init; }

        public bool HasPreviousPage { get; init; }

        public bool HasNextPage { get; init; }

        public PagedHttpResponse(IReadOnlyCollection<TItem> items, int totalCount, int pageNumber, int pageSize, int totalPages, bool hasPreviousPage, bool hasNextPage)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = totalPages;
            HasPreviousPage = hasPreviousPage;
            HasNextPage = hasNextPage;
        }
    }
}