namespace ESAM.GrowTracking.API.Controllers.UserSessions.GetUserSessions
{
    public record GetUserSessionsHttpResponse
    {
        public IReadOnlyCollection<UserSessionHttpResponse> UserSessions { get; init; }

        public GetUserSessionsHttpResponse(IReadOnlyCollection<UserSessionHttpResponse> userSessions)
        {
            UserSessions = userSessions;
        }

        public record UserSessionHttpResponse
        {
            public int UserSessionId { get; init; }

            public int UserId { get; init; }

            public int UserDeviceId { get; init; }

            public string DeviceName { get; init; }

            public string DeviceIdentifier { get; init; }

            public string ApiClientType { get; init; }

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

            public UserSessionWorkProfileSelectedHttpResponse UserSessionWorkProfileSelected { get; init; }

            public UserSessionHttpResponse(int userSessionId, int userId, int userDeviceId, string deviceName, string deviceIdentifier, string apiClientType, string? ipAddress,
                string? userAgent, bool isActive, bool isRevoked, DateTime? revokedAt, string? revokedReason, string? closedByUsername, DateTime createdAt,
                DateTime? lastActivityAt, DateTime expiresAt, DateTime absoluteExpiresAt, UserSessionWorkProfileSelectedHttpResponse userSessionWorkProfileSelected)
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

            public record UserSessionWorkProfileSelectedHttpResponse
            {
                public int WorkProfileId { get; init; }

                public string WorkProfile { get; init; }

                public string WorkProfileType { get; init; }

                public UserSessionWorkProfileRoleCampusSelectedHttpResponse? UserSessionWorkProfileRoleCampusSelected { get; init; }

                public UserSessionWorkProfileSelectedHttpResponse(int workProfileId, string workProfile, string workProfileType,
                    UserSessionWorkProfileRoleCampusSelectedHttpResponse? userSessionWorkProfileRoleCampusSelected)
                {
                    WorkProfileId = workProfileId;
                    WorkProfile = workProfile;
                    WorkProfileType = workProfileType;
                    UserSessionWorkProfileRoleCampusSelected = userSessionWorkProfileRoleCampusSelected;
                }

                public record UserSessionWorkProfileRoleCampusSelectedHttpResponse
                {
                    public int RoleId { get; init; }

                    public string Role { get; init; }

                    public int CampusId { get; init; }

                    public string Campus { get; init; }

                    public UserSessionWorkProfileRoleCampusSelectedHttpResponse(int roleId, string role, int campusId, string campus)
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