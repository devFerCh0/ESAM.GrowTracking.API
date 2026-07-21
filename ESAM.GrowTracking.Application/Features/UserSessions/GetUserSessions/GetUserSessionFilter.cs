using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public record GetUserSessionFilter
    {
        public int UserId { get; init; }

        public bool? IsActive { get; init; }

        public ApiClientType? ApiClientType { get; init; }

        public string? SearchTerm { get; init; }

        public GetUserSessionSortBy GetUserSessionSortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public DateTime UtcNow { get; init; }

        public GetUserSessionFilter(int userId, bool? isActive, ApiClientType? apiClientType, string? searchTerm, GetUserSessionSortBy getUserSessionSortBy, 
            SortDirection sortDirection, int pageNumber, int pageSize, DateTime utcNow)
        {
            UserId = userId;
            IsActive = isActive;
            ApiClientType = apiClientType;
            SearchTerm = searchTerm;
            GetUserSessionSortBy = getUserSessionSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
            UtcNow = utcNow;
        }
    }
}