using ESAM.GrowTracking.API.Abstractions.Security;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Results;
using System.Security.Claims;

namespace ESAM.GrowTracking.API.Security
{
    public sealed class JwtTokenValidatedHandler : IJwtTokenValidatedHandler
    {
        private readonly ILogger<JwtTokenValidatedHandler> _logger;
        private readonly IAccessTokenValidationService _accessTokenValidationService;

        public JwtTokenValidatedHandler(
            ILogger<JwtTokenValidatedHandler> logger,
            IAccessTokenValidationService accessTokenValidationService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(accessTokenValidationService);
            _logger = logger;
            _accessTokenValidationService = accessTokenValidationService;
        }

        public async Task<Result> HandleAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(principal);

            var accessTokenType = principal.GetAccessTokenType();
            if (accessTokenType is null)
            {
                _logger.LogWarning("JwtTokenValidatedHandler: tipo de token ausente o inválido en los claims.");
                return Result.Fail(Error.Unauthorized("El tipo de token no es válido o no está presente."));
            }

            var jti = principal.GetJti();
            if (string.IsNullOrWhiteSpace(jti))
            {
                _logger.LogWarning("JwtTokenValidatedHandler: JTI ausente o inválido en los claims.");
                return Result.Fail(Error.Unauthorized("El identificador único del token no es válido o no está presente."));
            }

            var userId = principal.GetUserId();
            if (userId is null)
            {
                _logger.LogWarning("JwtTokenValidatedHandler: identificador de usuario ausente o inválido en los claims.");
                return Result.Fail(Error.Unauthorized("El identificador de usuario no es válido o no está presente."));
            }

            var securityStamp = principal.GetSecurityStamp();
            if (string.IsNullOrWhiteSpace(securityStamp))
            {
                _logger.LogWarning(
                    "JwtTokenValidatedHandler: sello de seguridad ausente o inválido en los claims. UserId={UserId}",
                    userId.Value);
                return Result.Fail(Error.Unauthorized("El sello de seguridad no es válido o no está presente."));
            }

            var tokenVersion = principal.GetTokenVersion();
            if (tokenVersion is null)
            {
                _logger.LogWarning(
                    "JwtTokenValidatedHandler: versión de token ausente o inválida en los claims. UserId={UserId}",
                    userId.Value);
                return Result.Fail(Error.Unauthorized("La versión del token no es válida o no está presente."));
            }

            var userDeviceId = principal.GetUserDeviceId();
            if (userDeviceId is null)
            {
                _logger.LogWarning(
                    "JwtTokenValidatedHandler: identificador de dispositivo ausente o inválido en los claims. UserId={UserId}",
                    userId.Value);
                return Result.Fail(Error.Unauthorized("El identificador del dispositivo no es válido o no está presente."));
            }

            return await _accessTokenValidationService.ValidateAccessTokenAsync(
                accessTokenType.Value,
                jti,
                userId.Value,
                securityStamp,
                tokenVersion.Value,
                userDeviceId.Value,
                principal.GetUserSessionId(),
                principal.GetWorkProfileId(),
                principal.GetRoleId(),
                principal.GetCampusId(),
                cancellationToken);
        }
    }
}