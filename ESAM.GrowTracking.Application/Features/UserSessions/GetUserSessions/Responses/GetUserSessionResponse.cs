using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions.Responses
{
    public record GetUserSessionResponse
    {
        public int UserSessionId { get; init; }

        public int UserDeviceId { get; init; }

        public string DeviceName { get; init; }

        public string DeviceIdentifier { get; init; }

        public ApiClientType ApiClientType { get; init; }

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }

        public bool IsActive { get; init; }

        public bool IsRevoked { get; init; }

        public DateTime? RevokedAt { get; init; }

        public string? RevokedReason { get; init; }

        public string? ClosedByUsername { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? LastActivityAt { get; init; }

        public DateTime ExpiresAt { get; init; }

        public DateTime AbsoluteExpiresAt { get; init; }

        public GetUserSessionWorkProfileSelectedResponse? GetUserSessionWorkProfileSelected { get; init; }

        public GetUserSessionResponse(int userSessionId, int userDeviceId, string deviceName, string deviceIdentifier, ApiClientType apiClientType, string? ipAddress, 
            string? userAgent, bool isActive, bool isRevoked, DateTime? revokedAt, string? revokedReason, string? closedByUsername, DateTime createdAt, DateTime? lastActivityAt, 
            DateTime expiresAt, DateTime absoluteExpiresAt, GetUserSessionWorkProfileSelectedResponse? getUserSessionWorkProfileSelected)
        {
            UserSessionId = userSessionId;
            UserDeviceId = userDeviceId;
            DeviceName = deviceName;
            DeviceIdentifier = deviceIdentifier;
            ApiClientType = apiClientType;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            IsActive = isActive;
            IsRevoked = isRevoked;
            RevokedAt = revokedAt;
            RevokedReason = revokedReason;
            ClosedByUsername = closedByUsername;
            CreatedAt = createdAt;
            LastActivityAt = lastActivityAt;
            ExpiresAt = expiresAt;
            AbsoluteExpiresAt = absoluteExpiresAt;
            GetUserSessionWorkProfileSelected = getUserSessionWorkProfileSelected;
        }
    }
}