using ESAM.GrowTracking.Application.ValueObjects;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IAuthSessionCookieService
    {
        string? ResolveRefreshToken(string? bodyToken);

        string? SetSessionCookies(string refreshTokenRaw, DateTime refreshTokenExpiresAt);

        void ClearSessionCookies();

        bool RequiresCookieClearOnFailure(IReadOnlyList<Error> errors);
    }
}