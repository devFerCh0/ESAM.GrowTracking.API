namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAuthCookieService
    {
        void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt);

        void DeleteRefreshTokenCookie();

        bool TryGetRefreshToken(out string? refreshToken);

        void SetXsrfCookie(string xsrfToken, DateTimeOffset? absoluteExpiry = null);

        void DeleteXsrfCookie();

        string EffectiveRefreshCookieName();
    }
}