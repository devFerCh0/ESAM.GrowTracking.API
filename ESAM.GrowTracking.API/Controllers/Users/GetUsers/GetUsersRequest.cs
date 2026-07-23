namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers
{
    public record GetUsersRequest
    {
        public string? SearchTerm { get; init; }

        public bool? IsDeleted { get; init; }

        public bool? IsLocked { get; init; }

        public int? WorkProfileId { get; init; }

        public string? UsersSortBy { get; init; }

        public string? SortDirection { get; init; }

        public int? PageNumber { get; init; }

        public int? PageSize { get; init; }

        public GetUsersRequest(string? searchTerm, bool? isDeleted, bool? isLocked, int? workProfileId, string? usersSortBy, string? sortDirection, int? pageNumber, int? pageSize)
        {
            SearchTerm = searchTerm;
            IsDeleted = isDeleted;
            IsLocked = isLocked;
            WorkProfileId = workProfileId;
            UsersSortBy = usersSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}