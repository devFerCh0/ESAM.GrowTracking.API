namespace ESAM.GrowTracking.API.Controllers.UserDevices.GetUserDevices
{
    public record GetUserDevicesHttpResponse
    {
        public IReadOnlyCollection<UserDeviceHttpResponse> UserDevices { get; init; }

        public GetUserDevicesHttpResponse(IReadOnlyCollection<UserDeviceHttpResponse> userDevices)
        {
            UserDevices = userDevices;
        }

        public record UserDeviceHttpResponse
        {
            public int UserDeviceId { get; init; }

            public int UserId { get; init; }

            public string DeviceName { get; init; }

            public string DeviceIdentifier { get; init; }

            public string ApiClientType { get; init; }

            public bool IsLocked { get; init; }

            public DateTime? LockoutEndAt { get; init; }

            public int FailedLoginCount { get; init; }

            public DateTime? LastFailedLoginAt { get; init; }

            public DateTime? LastLoginAt { get; init; }

            public DateTime? LastSeenAt { get; init; }

            public string? LastIp { get; init; }

            public bool IsDeleted { get; init; }

            public bool HasActiveSessions { get; init; }

            public DateTime CreatedAt { get; init; }

            public UserDeviceHttpResponse(int userDeviceId, int userId, string deviceName, string deviceIdentifier, string apiClientType, bool isLocked, DateTime? lockoutEndAt,
                int failedLoginCount, DateTime? lastFailedLoginAt, DateTime? lastLoginAt, DateTime? lastSeenAt, string? lastIp, bool isDeleted, bool hasActiveSessions,
                DateTime createdAt)
            {
                UserDeviceId = userDeviceId;
                UserId = userId;
                DeviceName = deviceName;
                DeviceIdentifier = deviceIdentifier;
                ApiClientType = apiClientType;
                IsLocked = isLocked;
                LockoutEndAt = lockoutEndAt;
                FailedLoginCount = failedLoginCount;
                LastFailedLoginAt = lastFailedLoginAt;
                LastLoginAt = lastLoginAt;
                LastSeenAt = lastSeenAt;
                LastIp = lastIp;
                IsDeleted = isDeleted;
                HasActiveSessions = hasActiveSessions;
                CreatedAt = createdAt;
            }
        }
    }
}