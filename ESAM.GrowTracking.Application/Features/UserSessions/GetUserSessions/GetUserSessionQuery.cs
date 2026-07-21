using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public record GetUserSessionQuery : IRequest<Result<PagedResponse<GetUserSessionResponse>>>
    {
        public int UserId { get; init; }

        public bool? IsActive { get; init; }

        public ApiClientType? ApiClientType { get; init; }

        public string? SearchTerm { get; init; }

        public GetUserSessionSortBy GetUserSessionSortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public GetUserSessionQuery(int userId, bool? isActive = null, ApiClientType? apiClientType = null, string? searchTerm = null, 
            GetUserSessionSortBy getUserSessionSortBy = GetUserSessionSortBy.CreatedAt, SortDirection sortDirection = SortDirection.Descending, int pageNumber = 1, 
            int pageSize = 20)
        {
            UserId = userId;
            IsActive = isActive;
            ApiClientType = apiClientType;
            SearchTerm = searchTerm;
            GetUserSessionSortBy = getUserSessionSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}