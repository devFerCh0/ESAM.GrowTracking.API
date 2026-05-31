namespace ESAM.GrowTracking.Application.Abstractions.Services.Results
{
    public sealed class PurgeExpiredTokensResult
    {
        public int BlacklistedAccessTokensTemporaryDeleted { get; }

        public int BlacklistedAccessTokensPermanentDeleted { get; }

        public int BlacklistedRefreshTokensDeleted { get; }

        public int UserSessionRefreshTokensDeleted { get; }

        public int TotalDeleted => BlacklistedAccessTokensTemporaryDeleted + BlacklistedAccessTokensPermanentDeleted + BlacklistedRefreshTokensDeleted
            + UserSessionRefreshTokensDeleted;

        public DateTime PurgedAt { get; }

        public PurgeExpiredTokensResult(int blacklistedAccessTokensTemporaryDeleted, int blacklistedAccessTokensPermanentDeleted, int blacklistedRefreshTokensDeleted,
            int userSessionRefreshTokensDeleted, DateTime purgedAt)
        {
            BlacklistedAccessTokensTemporaryDeleted = blacklistedAccessTokensTemporaryDeleted;
            BlacklistedAccessTokensPermanentDeleted = blacklistedAccessTokensPermanentDeleted;
            BlacklistedRefreshTokensDeleted = blacklistedRefreshTokensDeleted;
            UserSessionRefreshTokensDeleted = userSessionRefreshTokensDeleted;
            PurgedAt = purgedAt;
        }
    }
}