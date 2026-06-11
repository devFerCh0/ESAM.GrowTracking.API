using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class BlacklistedRefreshToken : IEntity<int>
    {
        private BlacklistedRefreshToken() { }

        public int Id { get; private set; }

        public int UserSessionRefreshTokenId { get; private set; }

        public string Identifier { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }

        public DateTime BlacklistedAt { get; private set; }

        public string? Reason { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public int CreatedBy { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public UserSessionRefreshToken UserSessionRefreshToken { get; private set; } = null!;

        //public BlacklistedRefreshToken(int userSessionRefreshTokenId, string identifier, DateTime expiresAt, DateTime blacklistedAt, string reason, int createdBy, 
        //    DateTime? createdAt = null)
        //{
        //    UserSessionRefreshTokenId = userSessionRefreshTokenId;
        //    Identifier = identifier;
        //    ExpiresAt = expiresAt;
        //    BlacklistedAt = blacklistedAt;
        //    Reason = reason;
        //    CreatedAt = createdAt ?? DateTime.UtcNow;
        //    CreatedBy = createdBy;
        //}
    }
}