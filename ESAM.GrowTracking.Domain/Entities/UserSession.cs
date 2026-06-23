using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSession : AuditableEntity, IEntity<int>
    {
        private readonly List<UserSessionRefreshToken> _userSessionRefreshTokens = [];
        private readonly List<BlacklistedAccessTokenSession> _blacklistedAccessTokensSession = [];

        private UserSession() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public int UserDeviceId { get; private set; }

        public string? IpAddress { get; private set; }

        public string? UserAgent { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime AbsoluteExpiresAt { get; private set; }

        public DateTime? LastActivityAt { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public string? RevokedReason { get; private set; }

        public bool IsPersistent { get; private set; }

        public int? ClosedByUserId { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public UserDevice UserDevice { get; private set; } = null!;

        public UserSessionWorkProfileSelected? UserSessionWorkProfileSelected { get; private set; }

        public User? ClosedByUser { get; private set; }

        public IReadOnlyCollection<UserSessionRefreshToken> UserSessionRefreshTokens => _userSessionRefreshTokens.AsReadOnly();

        public IReadOnlyCollection<BlacklistedAccessTokenSession> BlacklistedAccessTokensSession => _blacklistedAccessTokensSession.AsReadOnly();

        public UserSession(int userId, int userDeviceId, string? ipAddress, string? userAgent, DateTime expiresAt, DateTime absoluteExpiresAt, bool isPersistent, int createdBy,
            DateTime? createdAt = null)
        {
            UserId = userId;
            UserDeviceId = userDeviceId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ExpiresAt = expiresAt;
            AbsoluteExpiresAt = absoluteExpiresAt;
            IsPersistent = isPersistent;
            SetCreatedAudit(createdBy, createdAt);
        }

        public void UpdateLastActivity(DateTime lastActivityAt, int updatedBy, DateTime? updatedAt = null)
        {
            LastActivityAt = lastActivityAt;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        //public void Revoke(DateTime revokedAt, string revokedReason, int closedByUserId, int updatedBy, DateTime? updatedAt = null)
        //{
        //    IsRevoked = true;
        //    RevokedAt = revokedAt;
        //    RevokedReason = revokedReason;
        //    ClosedByUserId = closedByUserId;
        //    SetUpdatedAudit(updatedBy, updatedAt);
        //}

        //public void UpdateExpiresAt(DateTime expiresAt, int updatedBy, DateTime? updatedAt = null)
        //{
        //    ExpiresAt = expiresAt;
        //    SetUpdatedAudit(updatedBy, updatedAt);
        //}
    }
}