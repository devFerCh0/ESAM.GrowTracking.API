using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public record GetUserSessionsFilter
    {
        public int UserId { get; init; }

        public bool? IsActive { get; init; }

        public ApiClientType? ApiClientType { get; init; }

        public string? SearchTerm { get; init; }

        public GetUserSessionsSortBy UserSessionsSortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public DateTime UtcNow { get; init; }

        public GetUserSessionsFilter(int userId, bool? isActive, ApiClientType? apiClientType, string? searchTerm, GetUserSessionsSortBy userSessionsSortBy, 
            SortDirection sortDirection, int pageNumber, int pageSize, DateTime utcNow)
        {
            UserId = userId;
            IsActive = isActive;
            ApiClientType = apiClientType;
            SearchTerm = searchTerm;
            UserSessionsSortBy = userSessionsSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
            UtcNow = utcNow;
        }
    }
}