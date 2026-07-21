using ESAM.GrowTracking.Application.Features.Commons;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers
{
    public record GetUsersFilter
    {
        public string? SearchTerm { get; init; }

        public bool? IsDeleted { get; init; }

        public bool? IsLocked { get; init; }

        public int? WorkProfileId { get; init; }

        public GetUsersSortBy SortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public DateTime UtcNow { get; init; }

        public GetUsersFilter(string? searchTerm, bool? isDeleted, bool? isLocked, int? workProfileId, GetUsersSortBy sortBy, SortDirection sortDirection, int pageNumber,
            int pageSize, DateTime utcNow)
        {
            SearchTerm = searchTerm;
            IsDeleted = isDeleted;
            IsLocked = isLocked;
            WorkProfileId = workProfileId;
            SortBy = sortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
            UtcNow = utcNow;
        }
    }
}