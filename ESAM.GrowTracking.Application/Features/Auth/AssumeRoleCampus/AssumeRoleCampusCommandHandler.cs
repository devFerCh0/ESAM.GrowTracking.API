using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusCommandHandler : IRequestHandler<AssumeRoleCampusCommand, Result<AssumeRoleCampusResponse>>
    {
        private readonly ILogger<AssumeRoleCampusCommandHandler> _logger;
        private readonly IValidator<AssumeRoleCampusCommand> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly IUserQuery _userQuery;
        private readonly IClientInfoService _clientInfoService;

        public AssumeRoleCampusCommandHandler(ILogger<AssumeRoleCampusCommandHandler> logger, IValidator<AssumeRoleCampusCommand> validator, 
            ICurrentUserService currentUserService, IDateTimeService dateTimeService, ICurrentUserValidatorService currentUserValidatorService, 
            IUserSessionService userSessionService, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, IUserQuery userQuery,
            IClientInfoService clientInfoService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(currentUserValidatorService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            ArgumentNullException.ThrowIfNull(userQuery);
            ArgumentNullException.ThrowIfNull(clientInfoService);
            _logger = logger;
            _validator = validator;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _currentUserValidatorService = currentUserValidatorService;
            _userSessionService = userSessionService;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            _userQuery = userQuery;
            _clientInfoService = clientInfoService;
        }

        public async Task<Result<AssumeRoleCampusResponse>> Handle(AssumeRoleCampusCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<AssumeRoleCampusResponse>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: intento de acceso no autenticado.");
                return Result<AssumeRoleCampusResponse>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            if (_currentUserService.AccessTokenType != AccessTokenType.Temporary)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: tipo de token de acceso inválido. Esperado=Temporal, Actual={AccessTokenType}", 
                    _currentUserService.AccessTokenType);
                return Result<AssumeRoleCampusResponse>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso temporal."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var currentUserResult = await _currentUserValidatorService.GetAndValidateCurrentUserAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (currentUserResult.IsFailure)
                return Result<AssumeRoleCampusResponse>.Fail(currentUserResult.Errors);
            var user = currentUserResult.Value;
            var currentUserDeviceValidationResult = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking,
                cancellationToken);
            if (currentUserDeviceValidationResult.IsFailure)
                return Result<AssumeRoleCampusResponse>.Fail(currentUserDeviceValidationResult.Errors);
            var workProfileTypeValidationResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId, request.WorkProfileId!.Value,
                WorkProfileType.WithRoles, asTracking, cancellationToken);
            if (workProfileTypeValidationResult.IsFailure)
                return Result<AssumeRoleCampusResponse>.Fail(workProfileTypeValidationResult.Errors);
            var roleCampusPermissionsValidationResult = await _currentUserValidatorService.ValidateUserRoleCampusAndHasPermissionsAsync(currentUserId, request.RoleId!.Value, 
                request.CampusId!.Value, asTracking, cancellationToken);
            if (roleCampusPermissionsValidationResult.IsFailure)
                return Result<AssumeRoleCampusResponse>.Fail(roleCampusPermissionsValidationResult.Errors);
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId, currentUserDeviceId, ipAddress, userAgent,
                request.WorkProfileId!.Value, utcNow, WorkProfileType.WithRoles, _currentUserService.Jti!, _currentUserService.AccessTokenExpiration!.Value, 
                _currentUserService.IsPersistent!.Value, request.RoleId!.Value, request.CampusId!.Value, asTracking, cancellationToken);
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, user.SecurityStamp, user.TokenVersion, currentUserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, request.WorkProfileId!.Value, request.RoleId!.Value, request.CampusId!.Value);
            var assumeRoleCampusUser = await _userQuery.GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(user.Id, userSession.Id, asTracking,cancellationToken);
            if (assumeRoleCampusUser is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, " + 
                    "UserSessionId={UserSessionId}", user.Id, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles is null || assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles.Count == 0)
            {
                _logger.LogError("AssumeRoleCampusCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", user.Id, 
                    userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses is null || assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses.Count == 0)
            {
                _logger.LogError("AssumeRoleCampusCommand: el usuario no tiene roles de sede asignados. UserId={UserId}, UserSessionId={UserSessionId}", user.Id, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene roles de sede asignados."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserSession is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", user.Id,  userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            return Result<AssumeRoleCampusResponse>.Ok(new AssumeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier, 
                refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt, assumeRoleCampusUser));
        }
    }
}