using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class BlacklistedAccessTokenPermanent : IEntity<int>
    {
        private BlacklistedAccessTokenPermanent() { }

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

        public BlacklistedAccessTokenPermanent(int userSessionId, string jti, DateTime expiresAt, DateTime blacklistedAt, string reason, int createdBy, DateTime? createdAt = null)
        {
            UserSessionId = userSessionId;
            Jti = jti;
            ExpiresAt = expiresAt;
            BlacklistedAt = blacklistedAt;
            Reason = reason;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            CreatedBy = createdBy;
        }

        //public BlacklistedAccessTokenPermanent(int userSessionId, string jti, DateTime expirationDate, string? reason, int? createdBy)
        //{
        //    UserSessionId = userSessionId;
        //    Jti = jti;
        //    ExpirationDate = expirationDate;
        //    BlacklistedAt = DateTime.UtcNow;
        //    CreatedAt = DateTime.UtcNow;
        //    Reason = reason;
        //    CreatedBy = createdBy;
        //}
    }
}