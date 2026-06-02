namespace ESAM.GrowTracking.Application.Abstractions.Services.Results
{
    public sealed class PurgeExpiredTokensResult(int blacklistedAccessTokensTemporaryDeleted, int blacklistedAccessTokensPermanentDeleted, int blacklistedRefreshTokensDeleted,
        int userSessionRefreshTokensDeleted, DateTime purgedAt)
    {
        public int BlacklistedAccessTokensTemporaryDeleted { get; } = blacklistedAccessTokensTemporaryDeleted;

        public int BlacklistedAccessTokensPermanentDeleted { get; } = blacklistedAccessTokensPermanentDeleted;

        public int BlacklistedRefreshTokensDeleted { get; } = blacklistedRefreshTokensDeleted;

        public int UserSessionRefreshTokensDeleted { get; } = userSessionRefreshTokensDeleted;

        public int TotalDeleted => BlacklistedAccessTokensTemporaryDeleted + BlacklistedAccessTokensPermanentDeleted + BlacklistedRefreshTokensDeleted
            + UserSessionRefreshTokensDeleted;

        public DateTime PurgedAt { get; } = purgedAt;
    }
}