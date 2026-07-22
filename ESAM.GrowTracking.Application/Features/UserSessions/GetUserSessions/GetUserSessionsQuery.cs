using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public record GetUserSessionsQuery : IRequest<Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>>
    {
        public int UserId { get; init; }

        public bool? IsActive { get; init; }

        public ApiClientType? ApiClientType { get; init; }

        public string? SearchTerm { get; init; }

        public GetUserSessionsSortBy UserSessionsSortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public GetUserSessionsQuery(int userId, bool? isActive = null, ApiClientType? apiClientType = null, string? searchTerm = null, 
            GetUserSessionsSortBy userSessionsSortBy = GetUserSessionsSortBy.CreatedAt, SortDirection sortDirection = SortDirection.Descending, int pageNumber = 1, 
            int pageSize = 20)
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