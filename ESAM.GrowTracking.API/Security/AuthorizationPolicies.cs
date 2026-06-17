namespace ESAM.GrowTracking.API.Security
{
    public static class AuthorizationPolicies
    {
        public const string RequireSessionTypeAccessToken = nameof(RequireSessionTypeAccessToken);
        public const string RequireTemporaryTypeAccessToken = nameof(RequireTemporaryTypeAccessToken);
    }
}