namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers
{
    public record GetUsersHttpResponse
    {
        public IReadOnlyCollection<UserHttpResponse> Users { get; init; }

        public GetUsersHttpResponse(IReadOnlyCollection<UserHttpResponse> users)
        {
            Users = users;
        }

        public record UserHttpResponse
        {
            public int UserId { get; init; }

            public string Username { get; init; }

            public string Email { get; init; }

            public string Fullname { get; init; }

            public bool IsLocked { get; init; }

            public DateTime? LockoutEndAt { get; init; }

            public bool IsDeleted { get; init; }

            public bool HasActiveSessions { get; init; }

            public DateTime CreatedAt { get; init; }

            public DateTime? UpdatedAt { get; init; }

            public IReadOnlyCollection<UserWorkProfileHttpResponse> UserWorkProfiles { get; init; }

            public UserHttpResponse(int userId, string username, string email, string fullname, bool isLocked, DateTime? lockoutEndAt, bool isDeleted, bool hasActiveSessions,
                DateTime createdAt, DateTime? updatedAt, IReadOnlyCollection<UserWorkProfileHttpResponse> userWorkProfiles)
            {
                UserId = userId;
                Username = username;
                Email = email;
                Fullname = fullname;
                IsLocked = isLocked;
                LockoutEndAt = lockoutEndAt;
                IsDeleted = isDeleted;
                HasActiveSessions = hasActiveSessions;
                CreatedAt = createdAt;
                UpdatedAt = updatedAt;
                UserWorkProfiles = userWorkProfiles;
            }

            public record UserWorkProfileHttpResponse
            {
                public int WorkProfileId { get; init; }

                public string WorkProfile { get; init; }

                public string WorkProfileType { get; init; }

                public UserWorkProfileHttpResponse(int workProfileId, string workProfile, string workProfileType)
                {
                    WorkProfileId = workProfileId;
                    WorkProfile = workProfile;
                    WorkProfileType = workProfileType;
                }
            }
        }
    }
}