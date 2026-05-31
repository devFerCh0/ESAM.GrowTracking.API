using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Abstractions.Security;
using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Http;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class AuthCookieService : IAuthCookieService
    {
        private readonly ILogger<AuthCookieService> _logger;
        private readonly CookieSettings _cookieSettings;
        private readonly IHostEnvironment _env;
        private readonly ITokenProtector _tokenProtector;
        private readonly IHttpRequestContextReader _requestReader;
        private readonly IResponseCookieWriter _responseWriter;

        public AuthCookieService(ILogger<AuthCookieService> logger, IOptions<CookieSettings> cookieSettingsOptions, IHostEnvironment env, ITokenProtector tokenProtector,
            IHttpRequestContextReader requestReader, IResponseCookieWriter responseWriter)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(cookieSettingsOptions);
            ArgumentNullException.ThrowIfNull(env);
            ArgumentNullException.ThrowIfNull(tokenProtector);
            ArgumentNullException.ThrowIfNull(requestReader);
            ArgumentNullException.ThrowIfNull(responseWriter);
            _logger = logger;
            _cookieSettings = cookieSettingsOptions.Value ?? throw new ArgumentNullException(nameof(cookieSettingsOptions));
            _env = env;
            _tokenProtector = tokenProtector;
            _requestReader = requestReader;
            _responseWriter = responseWriter;
        }

        public string EffectiveRefreshCookieName() => _cookieSettings.EffectiveRefreshCookieName();

        public void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("SetRefreshTokenCookie: se proporcionó un token vacío, operación omitida.");
                return;
            }
            try
            {
                var protectedToken = _tokenProtector.Protect(refreshToken);
                var descriptor = BuildCookieDescriptor(expiresAt, true);
                _responseWriter.Append(_cookieSettings.EffectiveRefreshCookieName(), protectedToken, descriptor);
            }
            catch (CryptographicException ce)
            {
                _logger.LogError(ce, "SetRefreshTokenCookie: error al proteger el refresh token.");
                throw;
            }
        }

        public void DeleteRefreshTokenCookie()
        {
            var descriptor = BuildCookieDescriptor(DateTimeOffset.UtcNow.AddDays(-1), true);
            try
            {
                _responseWriter.Delete(_cookieSettings.EffectiveRefreshCookieName(), descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeleteRefreshTokenCookie: error al eliminar la cookie del refresh token (no crítico).");
            }
        }

        public bool TryGetRefreshToken(out string? refreshToken)
        {
            refreshToken = null;
            var cookieVal = _requestReader.GetCookieValue(_cookieSettings.EffectiveRefreshCookieName());
            if (!string.IsNullOrWhiteSpace(cookieVal))
            {
                if (_tokenProtector.TryUnprotect(cookieVal, out var unprotected) && !string.IsNullOrWhiteSpace(unprotected))
                {
                    refreshToken = unprotected;
                    return true;
                }
                _logger.LogWarning("TryGetRefreshToken: error al desproteger la cookie (token alterado o clave rotada). Token ignorado.");
                return false;
            }
            if (_cookieSettings.AllowRefreshTokenHeader)
            {
                var raw = _requestReader.GetHeaderValue("X-Refresh-Token")?.Trim();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    refreshToken = raw;
                    return true;
                }
            }
            return false;
        }

        public void SetXsrfCookie(string xsrfToken, DateTimeOffset? absoluteExpiry = null)
        {
            if (string.IsNullOrWhiteSpace(xsrfToken))
            {
                _logger.LogWarning("SetXsrfCookie: se proporcionó un token vacío, operación omitida.");
                return;
            }
            var expiry = absoluteExpiry ?? DateTimeOffset.UtcNow.AddMinutes(_cookieSettings.XsrfCookieExpiresMinutes);
            var descriptor = BuildCookieDescriptor(expiry, false);
            try
            {
                _responseWriter.Append(_cookieSettings.EffectiveXsrfCookieName(), xsrfToken, descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SetXsrfCookie: error al escribir la cookie XSRF (no crítico).");
            }
        }

        public void DeleteXsrfCookie()
        {
            var descriptor = BuildCookieDescriptor(DateTimeOffset.UtcNow.AddDays(-1), false);
            try
            {
                _responseWriter.Delete(_cookieSettings.EffectiveXsrfCookieName(), descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DeleteXsrfCookie: error al eliminar la cookie XSRF (no crítico).");
            }
        }

        private bool ShouldUseSecureFlag() => _env.IsProduction() || _cookieSettings.AlwaysSecure;

        private ResponseCookieDescriptor BuildCookieDescriptor(DateTimeOffset expiresAt, bool httpOnly)
        {
            var secure = ShouldUseSecureFlag();
            var sameSite = _cookieSettings.SameSite;
            if (sameSite == SameSitePolicy.None && !secure)
            {
                _logger.LogWarning("BuildCookieDescriptor: SameSite=None detectado con Secure=false. Se fuerza Secure=true para cumplir con RFC 6265bis. " +
                    "Configure AlwaysSecure=true o use SameSite=Lax en entornos sin HTTPS.");
                secure = true;
            }
            if (_cookieSettings.UseHostPrefix)
                return new ResponseCookieDescriptor
                {
                    HttpOnly = httpOnly,
                    Secure = true,
                    SameSite = sameSite,
                    Expires = expiresAt,
                    Path = "/",
                    Domain = null,
                    IsEssential = true
                };
            return new ResponseCookieDescriptor
            {
                HttpOnly = httpOnly,
                Secure = secure,
                SameSite = sameSite,
                Expires = expiresAt,
                Path = string.IsNullOrWhiteSpace(_cookieSettings.Path) ? "/" : _cookieSettings.Path,
                Domain = string.IsNullOrWhiteSpace(_cookieSettings.Domain) ? null : _cookieSettings.Domain,
                IsEssential = true
            };
        }
    }
}