using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers
{
    public record GetUsersQuery : IRequest<Result<PagedResult<GetUsersResponse>>>
    {
        public string? SearchTerm { get; init; }

        public bool? IsDeleted { get; init; }

        public bool? IsLocked { get; init; }

        public int? WorkProfileId { get; init; }

        public GetUsersSortBy SortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public GetUsersQuery(string? searchTerm = null, bool? isDeleted = null, bool? isLocked = null, int? workProfileId = null, GetUsersSortBy sortBy = GetUsersSortBy.Username, 
            SortDirection sortDirection = SortDirection.Ascending, int pageNumber = 1, int pageSize = 20)
        {
            SearchTerm = searchTerm;
            IsDeleted = isDeleted;
            IsLocked = isLocked;
            WorkProfileId = workProfileId;
            SortBy = sortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}