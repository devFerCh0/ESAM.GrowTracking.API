using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class BlacklistedAccessTokenSession : IEntity<int>
    {
        private BlacklistedAccessTokenSession() { }

        public int Id { get; private set; }

        public int UserSessionId { get; private set; }

        public string Jti { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime BlacklistedAt { get; private set; }

        public string? Reason { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public int CreatedBy { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSession UserSession { get; private set; } = null!;

        //public BlacklistedAccessTokenSession(int userSessionId, string jti, DateTime expiresAt, DateTime blacklistedAt, string reason, int createdBy, DateTime? createdAt = null)
        //{
        //    UserSessionId = userSessionId;
        //    Jti = jti;
        //    ExpiresAt = expiresAt;
        //    BlacklistedAt = blacklistedAt;
        //    Reason = reason;
        //    CreatedAt = createdAt ?? DateTime.UtcNow;
        //    CreatedBy = createdBy;
        //}
    }
}