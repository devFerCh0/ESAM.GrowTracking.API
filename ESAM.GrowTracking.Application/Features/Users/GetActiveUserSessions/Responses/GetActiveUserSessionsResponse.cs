using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Users.GetActiveUserSessions.Responses
{
    public record GetActiveUserSessionsResponse
    {
        public int UserSessionId { get; init; }

        public int UserId { get; init; }

        public int UserDeviceId { get; init; }

        public string DeviceName { get; init; }

        public ApiClientType ApiClientType { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public bool IsPersistent { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? LastActivityAt { get; init; }

        public DateTime ExpiresAt { get; init; }

        public DateTime AbsoluteExpiresAt { get; init; }

        public GetActiveUserSessionWorkProfileResponse? GetActiveUserSessionWorkProfile { get; init; }

        public GetActiveUserSessionsResponse(int userSessionId, int userId, int userDeviceId, string deviceName, ApiClientType apiClientType, string? ipAddress, string? userAgent,
            bool isPersistent, DateTime createdAt, DateTime? lastActivityAt, DateTime expiresAt, DateTime absoluteExpiresAt, 
            GetActiveUserSessionWorkProfileResponse? getActiveUserSessionWorkProfile)
        {
            UserSessionId = userSessionId;
            UserId = userId;
            UserDeviceId = userDeviceId;
            DeviceName = deviceName;
            ApiClientType = apiClientType;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            IsPersistent = isPersistent;
            CreatedAt = createdAt;
            LastActivityAt = lastActivityAt;
            ExpiresAt = expiresAt;
            AbsoluteExpiresAt = absoluteExpiresAt;
            GetActiveUserSessionWorkProfile = getActiveUserSessionWorkProfile;
        }
    }
}