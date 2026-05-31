using ESAM.GrowTracking.Application.ValueObjects;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAuthSessionCookieService
    {
        string? ResolveRefreshToken(string? bodyToken);

        void SetSessionCookies(string refreshTokenRaw, DateTime refreshTokenExpiresAt, DateTime accessTokenExpiresAt);

        void ClearSessionCookies();

        bool RequiresCookieClearOnFailure(IReadOnlyList<Error> errors);
    }
}