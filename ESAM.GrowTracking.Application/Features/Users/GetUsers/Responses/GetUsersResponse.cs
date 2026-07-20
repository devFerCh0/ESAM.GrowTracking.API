using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses
{
    public record GetUsersResponse
    {
        public int UserId { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Fullname { get; init; }

        public string IdentityDocument { get; init; }

        public IdentityDocumentType IdentityDocumentType { get; init; }

        public bool IsEmailConfirmed { get; init; }

        public bool IsLocked { get; init; }

        public DateTime? LockoutEndAt { get; init; }

        public bool IsDeleted { get; init; }

        public bool HasActiveSessions { get; init; }

        public DateTime CreatedAt { get; init; }

        public IReadOnlyCollection<GetUsersUserWorkProfileResponse> UserWorkProfiles { get; init; }

        public GetUsersResponse(int userId, string username, string email, string fullname, string identityDocument, IdentityDocumentType identityDocumentType,
            bool isEmailConfirmed, bool isLocked, DateTime? lockoutEndAt, bool isDeleted, bool hasActiveSessions, DateTime createdAt,
            IReadOnlyCollection<GetUsersUserWorkProfileResponse> userWorkProfiles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Fullname = fullname;
            IdentityDocument = identityDocument;
            IdentityDocumentType = identityDocumentType;
            IsEmailConfirmed = isEmailConfirmed;
            IsLocked = isLocked;
            LockoutEndAt = lockoutEndAt;
            IsDeleted = isDeleted;
            HasActiveSessions = hasActiveSessions;
            CreatedAt = createdAt;
            UserWorkProfiles = userWorkProfiles;
        }
    }
}