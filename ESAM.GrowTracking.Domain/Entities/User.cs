using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class User : AuditableEntity, IEntity<int>
    {
        private readonly List<UserWorkProfile> _userWorkProfiles = [];
        private readonly List<UserRoleCampus> _userRoleCampuses = [];
        private readonly List<BlacklistedAccessTokenTemporary> _blacklistedAccessTokensTemporary = [];
        private readonly List<UserSession> _userSessions = [];
        private readonly List<UserSession> _sessionsClosedByUser = [];
        private readonly List<UserPhoto> _userPhotos = [];
        private readonly List<UserDevice> _userDevices = [];

        private User() { }

        public int Id { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string NormalizedUserName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string NormalizedEmail { get; private set; } = string.Empty;

        public string Salt { get; private set; } = string.Empty;

        public string PasswordHash { get; private set; } = string.Empty;

        public string SecurityStamp { get; private set; } = string.Empty;

        public int TokenVersion { get; private set; }

        public bool IsEmailConfirmed { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTime? LockoutEndAt { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public Person Person { get; private set; } = null!;

        public IReadOnlyCollection<UserWorkProfile> UserWorkProfiles => _userWorkProfiles.AsReadOnly();

        public IReadOnlyCollection<UserRoleCampus> UserRoleCampuses => _userRoleCampuses.AsReadOnly();

        public IReadOnlyCollection<BlacklistedAccessTokenTemporary> BlacklistedAccessTokensTemporary => _blacklistedAccessTokensTemporary.AsReadOnly();

        public IReadOnlyCollection<UserSession> UserSessions => _userSessions.AsReadOnly();

        public IReadOnlyCollection<UserSession> SessionsClosedByUser => _sessionsClosedByUser.AsReadOnly();

        public IReadOnlyCollection<UserPhoto> UserPhotos => _userPhotos.AsReadOnly();

        public IReadOnlyCollection<UserDevice> UserDevices => _userDevices.AsReadOnly();

        public User(int id, string username, string email, string salt, string passwordHash, string securityStamp, int createdBy, DateTime? createdAt = null)
        {
            Id = id;
            Username = username.Trim();
            NormalizedUserName = username.Trim().ToUpperInvariant();
            Email = email.Trim();
            NormalizedEmail = email.Trim().ToUpperInvariant();
            Salt = salt;
            PasswordHash = passwordHash;
            SecurityStamp = securityStamp;
            SetCreatedAudit(createdBy, createdAt);
        }

        public bool IsLocked(DateTime utcNow) => LockoutEndAt.HasValue && LockoutEndAt.Value > utcNow;

        public void UpdateSecurityCredentials(string securityStamp, int tokenVersion, int updatedBy, DateTime? updatedAt = null)
        {
            SecurityStamp = securityStamp;
            TokenVersion = tokenVersion;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void ChangePassword(string salt, string passwordHash, int updatedBy, DateTime? updatedAt = null)
        {
            Salt = salt;
            PasswordHash = passwordHash;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void Lock(DateTime lockoutEndAt, int updatedBy, DateTime? updatedAt = null)
        {
            LockoutEndAt = lockoutEndAt;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void Unlock(int updatedBy, DateTime? updatedAt = null)
        {
            LockoutEndAt = null;
            SetUpdatedAudit(updatedBy, updatedAt);
        }
    }
}