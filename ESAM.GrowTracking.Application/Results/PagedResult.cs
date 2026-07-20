namespace ESAM.GrowTracking.Application.Results
{
    public sealed record PagedResult<TItem>(IReadOnlyCollection<TItem> Items, int TotalCount, int PageNumber, int PageSize)
    {
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}