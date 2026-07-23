namespace ESAM.GrowTracking.API.Controllers.UserSessions.GetUserSessions
{
    public record GetUserSessionsRequest
    {
        public int? UserId { get; init; }

        public bool? IsActive { get; init; }

        public string? ApiClientType { get; init; }

        public string? SearchTerm { get; init; }

        public string? UserSessionsSortBy { get; init; }

        public string? SortDirection { get; init; }

        public int? PageNumber { get; init; }

        public int? PageSize { get; init; }

        public GetUserSessionsRequest(int? userId, bool? isActive, string? apiClientType, string? searchTerm, string? userSessionsSortBy, string? sortDirection, int? pageNumber, 
            int? pageSize)
        {
            UserId = userId;
            IsActive = isActive;
            ApiClientType = apiClientType;
            SearchTerm = searchTerm;
            UserSessionsSortBy = userSessionsSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}