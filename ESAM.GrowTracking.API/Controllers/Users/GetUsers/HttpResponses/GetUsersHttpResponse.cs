namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers.HttpResponses
{
    public record GetUsersHttpResponse
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

        public IReadOnlyCollection<GetUsersUserWorkProfileHttpResponse> UserWorkProfiles { get; init; }

        public GetUsersHttpResponse(int userId, string username, string email, string fullname, bool isLocked, DateTime? lockoutEndAt, bool isDeleted, bool hasActiveSessions, 
            DateTime createdAt, DateTime? updatedAt, IReadOnlyCollection<GetUsersUserWorkProfileHttpResponse> userWorkProfiles)
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
    }
}