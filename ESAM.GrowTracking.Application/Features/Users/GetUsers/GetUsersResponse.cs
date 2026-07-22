using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers
{
    public record GetUsersResponse
    {
        public IReadOnlyCollection<UserResponse> Users { get; init; }

        public GetUsersResponse(IReadOnlyCollection<UserResponse> users)
        {
            Users = users;
        }

        public record UserResponse
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

            public IReadOnlyCollection<UserWorkProfileResponse> UserWorkProfiles { get; init; }

            public UserResponse(int userId, string username, string email, string fullname, bool isLocked, DateTime? lockoutEndAt, bool isDeleted, bool hasActiveSessions,
                DateTime createdAt, DateTime? updatedAt, IReadOnlyCollection<UserWorkProfileResponse> userWorkProfiles)
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

            public record UserWorkProfileResponse
            {
                public int WorkProfileId { get; init; }

                public string WorkProfile { get; init; }

                public WorkProfileType WorkProfileType { get; init; }

                public UserWorkProfileResponse(int workProfileId, string workProfile, WorkProfileType workProfileType)
                {
                    WorkProfileId = workProfileId;
                    WorkProfile = workProfile;
                    WorkProfileType = workProfileType;
                }
            }
        }
    }
}