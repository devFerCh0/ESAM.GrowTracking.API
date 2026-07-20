namespace ESAM.GrowTracking.Application.Features.Commons
{
    public sealed record PagedResponse<TItem>
    {
        public IReadOnlyCollection<TItem> Items { get; init; }

        public int TotalCount { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalPages { get; init; }

        public bool HasPreviousPage { get; init; }

        public bool HasNextPage { get; init; }

        public PagedResponse(IReadOnlyCollection<TItem> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
            HasPreviousPage = pageNumber > 1;
            HasNextPage = pageNumber < TotalPages;
        }
    }
}