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
            Items = items ?? [];
            TotalCount = totalCount < 0 ? 0 : totalCount;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 1 : pageSize;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < TotalPages;
        }
    }
}