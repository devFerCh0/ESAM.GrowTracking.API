using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class BlacklistedAccessTokenTemporary : IEntity<int>
    {
        private BlacklistedAccessTokenTemporary() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public string Jti { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime BlacklistedAt { get; private set; }

        public string? Reason { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public int CreatedBy { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public BlacklistedAccessTokenTemporary(int userId, string jti, DateTime expiresAt, DateTime blacklistedAt, string reason, int createdBy, DateTime? createdAt = null)
        {
            UserId = userId;
            Jti = jti;
            ExpiresAt = expiresAt;
            BlacklistedAt = blacklistedAt;
            Reason = reason;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            CreatedBy = createdBy;
        }
    }
}