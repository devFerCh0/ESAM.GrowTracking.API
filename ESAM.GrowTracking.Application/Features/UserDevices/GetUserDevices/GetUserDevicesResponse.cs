using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public record GetUserDevicesResponse
    {
        public IReadOnlyCollection<UserDeviceResponse> UserDevices { get; init; }

        public GetUserDevicesResponse(IReadOnlyCollection<UserDeviceResponse> userDevices)
        {
            UserDevices = userDevices;
        }

        public record UserDeviceResponse
        {
            public int UserDeviceId { get; init; }

            public int UserId { get; init; }

            public string DeviceName { get; init; }

            public string DeviceIdentifier { get; init; }

            public ApiClientType ApiClientType { get; init; }

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

            public UserDeviceResponse(int userDeviceId, int userId, string deviceName, string deviceIdentifier, ApiClientType apiClientType, bool isLocked, DateTime? lockoutEndAt,
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