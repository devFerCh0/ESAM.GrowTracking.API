using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSessionRefreshToken : AuditableEntity, IEntity<int>
    {
        private readonly List<BlacklistedRefreshToken> _blacklistedRefreshTokens = [];

        private UserSessionRefreshToken() { }

        public int Id { get; private set; }

        public int UserSessionId { get; private set; }

        public string Identifier { get; private set; } = string.Empty;

        public string Salt { get; private set; } = string.Empty;

        public string TokenHash { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime? LastUsedAt { get; private set; }

        public int RotationCount { get; private set; }

        public int? ReplacedByUserSessionRefreshTokenId { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public string? RevokedReason { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSession UserSession { get; private set; } = null!;

        public UserSessionRefreshToken? ReplacedByUserSessionRefreshToken { get; private set; }

        public UserSessionRefreshToken? ReplacesUserSessionRefreshToken { get; private set; }

        public IReadOnlyCollection<BlacklistedRefreshToken> BlacklistedRefreshTokens => _blacklistedRefreshTokens.AsReadOnly();

        public UserSessionRefreshToken(string identifier, string salt, string tokenHash, DateTime expiresAt, int createdBy, DateTime? createdAt = null)
        {
            Identifier = identifier;
            Salt = salt;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            SetCreatedAudit(createdBy, createdAt);
        }

        //public UserSessionRefreshToken(string identifier, string salt, string tokenHash, DateTime expiresAt, int createdBy, int userSessionId = 0, int rotationCount = 0, 
        //    DateTime? createdAt = null)
        //{
        //    UserSessionId = userSessionId;
        //    Identifier = identifier;
        //    Salt = salt;
        //    TokenHash = tokenHash;
        //    ExpiresAt = expiresAt;
        //    RotationCount = rotationCount;
        //    SetCreatedAudit(createdBy, createdAt);
        //}

        public void UpdateLastUsedAt(DateTime lastUsedAt, int updatedBy, DateTime? updatedAt = null)
        {
            LastUsedAt = lastUsedAt;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }

        public void Revoke(DateTime revokedAt, string revokedReason, int updatedBy, DateTime? updatedAt = null)
        {
            IsRevoked = true;
            RevokedAt = revokedAt;
            RevokedReason = revokedReason;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        //public void UpdateReplacedByUserSessionRefreshTokenId(int replacedByUserSessionRefreshTokenId)
        //{
        //    ReplacedByUserSessionRefreshTokenId = replacedByUserSessionRefreshTokenId;
        //}
    }
}