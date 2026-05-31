using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class AuthSessionCookieService : IAuthSessionCookieService
    {
        private readonly ILogger<AuthSessionCookieService> _logger;
        private readonly IAuthCookieService _authCookieService;

        public AuthSessionCookieService(ILogger<AuthSessionCookieService> logger, IAuthCookieService authCookieService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(authCookieService);
            _logger = logger;
            _authCookieService = authCookieService;
        }

        public string? ResolveRefreshToken(string? bodyToken)
        {
            if (!string.IsNullOrWhiteSpace(bodyToken))
                return bodyToken;
            return _authCookieService.TryGetRefreshToken(out var cookieToken) ? cookieToken : null;
        }

        public void SetSessionCookies(string refreshTokenRaw, DateTime refreshTokenExpiresAt, DateTime accessTokenExpiresAt)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenRaw))
                return;
            _authCookieService.SetRefreshTokenCookie(refreshTokenRaw, new DateTimeOffset(DateTime.SpecifyKind(refreshTokenExpiresAt, DateTimeKind.Utc)));
            _authCookieService.SetXsrfCookie(GenerateXsrfToken(), new DateTimeOffset(DateTime.SpecifyKind(accessTokenExpiresAt, DateTimeKind.Utc)));
        }

        public void ClearSessionCookies()
        {
            _authCookieService.DeleteRefreshTokenCookie();
            _authCookieService.DeleteXsrfCookie();
        }

        public bool RequiresCookieClearOnFailure(IReadOnlyList<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return errors.Any(static e => e.ErrorType is ErrorType.Unauthorized or ErrorType.Forbidden or ErrorType.Locked);
        }

        private static string GenerateXsrfToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}