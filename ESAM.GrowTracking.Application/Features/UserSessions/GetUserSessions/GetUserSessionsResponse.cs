using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public record GetUserSessionsResponse
    {
        public IReadOnlyCollection<UserSessionResponse> UserSessions { get; init; }

        public GetUserSessionsResponse(IReadOnlyCollection<UserSessionResponse> userSessions)
        {
            UserSessions = userSessions;
        }

        public record UserSessionResponse
        {
            public int UserSessionId { get; init; }

            public int UserId { get; init; }

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

            public UserSessionWorkProfileSelectedResponse? UserSessionWorkProfileSelected { get; init; }

            public UserSessionResponse(int userSessionId, int userId, int userDeviceId, string deviceName, string deviceIdentifier, ApiClientType apiClientType, string? ipAddress,
                string? userAgent, bool isActive, bool isRevoked, DateTime? revokedAt, string? revokedReason, string? closedByUsername, DateTime createdAt, 
                DateTime? lastActivityAt, DateTime expiresAt, DateTime absoluteExpiresAt, UserSessionWorkProfileSelectedResponse? userSessionWorkProfileSelected)
            {
                UserSessionId = userSessionId;
                UserId = userId;
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
                UserSessionWorkProfileSelected = userSessionWorkProfileSelected;
            }

            public record UserSessionWorkProfileSelectedResponse
            {
                public int WorkProfileId { get; init; }

                public string WorkProfile { get; init; }

                public WorkProfileType WorkProfileType { get; init; }

                public UserSessionWorkProfileRoleCampusSelectedResponse? UserSessionWorkProfileRoleCampusSelected { get; init; }

                public UserSessionWorkProfileSelectedResponse(int workProfileId, string workProfile, WorkProfileType workProfileType,
                    UserSessionWorkProfileRoleCampusSelectedResponse? userSessionWorkProfileRoleCampusSelected)
                {
                    WorkProfileId = workProfileId;
                    WorkProfile = workProfile;
                    WorkProfileType = workProfileType;
                    UserSessionWorkProfileRoleCampusSelected = userSessionWorkProfileRoleCampusSelected;
                }

                public record UserSessionWorkProfileRoleCampusSelectedResponse
                {
                    public int RoleId { get; init; }

                    public string Role { get; init; }

                    public int CampusId { get; init; }

                    public string Campus { get; init; }

                    public UserSessionWorkProfileRoleCampusSelectedResponse(int roleId, string role, int campusId, string campus)
                    {
                        RoleId = roleId;
                        Role = role;
                        CampusId = campusId;
                        Campus = campus;
                    }
                }
            }
        }
    }
}