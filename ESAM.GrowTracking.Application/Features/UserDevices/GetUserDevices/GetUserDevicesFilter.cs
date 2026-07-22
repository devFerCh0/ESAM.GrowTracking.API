using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public record GetUserDevicesFilter
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

        public DateTime UtcNow { get; init; }

        public GetUserDevicesFilter(int userId, bool? isLocked, ApiClientType? apiClientType, bool? isDeleted, string? searchTerm, GetUserDevicesSortBy userDevicesSortBy,
            SortDirection sortDirection, int pageNumber, int pageSize, DateTime utcNow)
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
            UtcNow = utcNow;
        }
    }
}