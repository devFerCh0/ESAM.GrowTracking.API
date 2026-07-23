using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Enums;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public record GetUserDevicesQuery : IRequest<Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>>
    {
        public int UserId { get; init; }

        public bool? IsLocked { get; init; }

        public ApiClientType? ApiClientType { get; init; }

        public bool? IsDeleted { get; init; }

        public string? SearchTerm { get; init; }

        public GetUserDevicesSortBy UserDevicesSortBy { get; init; }

        public SortDirection SortDirection { get; init; }

        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public GetUserDevicesQuery(int userId, bool? isLocked = null, ApiClientType? apiClientType = null, bool? isDeleted = null, string? searchTerm = null, 
            GetUserDevicesSortBy userDevicesSortBy = GetUserDevicesSortBy.LastSeenAt, SortDirection sortDirection = SortDirection.Descending, int pageNumber = 1, 
            int pageSize = 20)
        {
            UserId = userId;
            IsLocked = isLocked;
            ApiClientType = apiClientType;
            IsDeleted = isDeleted;
            SearchTerm = searchTerm;
            UserDevicesSortBy = userDevicesSortBy;
            SortDirection = sortDirection;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}