using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices
{
    public record GetLockedUserDeviceResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public int UserDeviceId { get; init; }

        public string DeviceIdentifier { get; init; }

        public string DeviceName { get; init; }

        public ApiClientType ApiClientType { get; init; }

        public int FailedLoginCount { get; init; }

        public DateTime? LastFailedLoginAt { get; init; }

        public DateTime LockoutEndAt { get; init; }

        public string? LastIp { get; init; }

        public string? LastUserAgent { get; init; }

        public DateTime? LastSeenAt { get; init; }

        public GetLockedUserDeviceResponse(int userId, string username, string email, string fullname, int userDeviceId, string deviceIdentifier, string deviceName,
            ApiClientType apiClientType, int failedLoginCount, DateTime? lastFailedLoginAt, DateTime lockoutEndAt, string? lastIp, string? lastUserAgent,
            DateTime? lastSeenAt)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            UserDeviceId = userDeviceId;
            DeviceIdentifier = deviceIdentifier;
            DeviceName = deviceName;
            ApiClientType = apiClientType;
            FailedLoginCount = failedLoginCount;
            LastFailedLoginAt = lastFailedLoginAt;
            LockoutEndAt = lockoutEndAt;
            LastIp = lastIp;
            LastUserAgent = lastUserAgent;
            LastSeenAt = lastSeenAt;
        }
    }
}