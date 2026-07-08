using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses
{
    public record GetActiveCurrentUserSessionsResponse
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

        public GetActiveCurrentUserSessionWorkProfileResponse? GetActiveCurrentUserSessionWorkProfile { get; init; }

        public bool IsCurrent { get; init; }

        public GetActiveCurrentUserSessionsResponse(int userSessionId, int userId, int userDeviceId, string deviceName, ApiClientType apiClientType, string? ipAddress,
            string? userAgent, bool isPersistent, DateTime createdAt, DateTime? lastActivityAt, DateTime expiresAt, DateTime absoluteExpiresAt, bool isCurrent,
            GetActiveCurrentUserSessionWorkProfileResponse? getActiveCurrentUserSessionWorkProfile)
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
            IsCurrent = isCurrent;
            GetActiveCurrentUserSessionWorkProfile = getActiveCurrentUserSessionWorkProfile;
        }
    }
}