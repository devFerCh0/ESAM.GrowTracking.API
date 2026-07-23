namespace ESAM.GrowTracking.API.Controllers.UserDevices.GetUserDevices
{
    public record GetUserDevicesRequest
    {
        public int? UserId { get; init; }

        public bool? IsLocked { get; init; }

        public string? ApiClientType { get; init; }

        public bool? IsDeleted { get; init; }

        public string? SearchTerm { get; init; }

        public string? UserDevicesSortBy { get; init; }

        public string? SortDirection { get; init; }

        public int? PageNumber { get; init; }

        public int? PageSize { get; init; }

        public GetUserDevicesRequest(int? userId, bool? isLocked, string? apiClientType, bool? isDeleted, string? searchTerm, string? userDevicesSortBy, string? sortDirection,
            int? pageNumber, int? pageSize)
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