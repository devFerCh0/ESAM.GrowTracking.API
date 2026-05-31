using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileCommandHandler : IRequestHandler<AssumeWorkProfileCommand, Result<AssumeWorkProfileResponse>>
    {
        private readonly ILogger<AssumeWorkProfileCommandHandler> _logger;
        private readonly IValidator<AssumeWorkProfileCommand> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly IUserQuery _userQuery;
        private readonly IClientInfoService _clientInfoService;

        public AssumeWorkProfileCommandHandler(ILogger<AssumeWorkProfileCommandHandler> logger, IValidator<AssumeWorkProfileCommand> validator,
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

        public async Task<Result<AssumeWorkProfileResponse>> Handle(AssumeWorkProfileCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<AssumeWorkProfileResponse>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: intento de acceso no autenticado.");
                return Result<AssumeWorkProfileResponse>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            if (_currentUserService.AccessTokenType != AccessTokenType.Temporary)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: tipo de token de acceso inválido. Esperado=Temporal, Actual={AccessTokenType}", _currentUserService.AccessTokenType);
                return Result<AssumeWorkProfileResponse>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso temporal."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var currentUserResult = await _currentUserValidatorService.GetAndValidateCurrentUserAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (currentUserResult.IsFailure)
                return Result<AssumeWorkProfileResponse>.Fail(currentUserResult.Errors);
            var user = currentUserResult.Value;
            var currentUserDeviceValidationResult = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking,
                cancellationToken);
            if (currentUserDeviceValidationResult.IsFailure)
                return Result<AssumeWorkProfileResponse>.Fail(currentUserDeviceValidationResult.Errors);
            var workProfilePermissionsValidationResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(currentUserId, 
                request.WorkProfileId!.Value, WorkProfileType.OnlyWorkProfile, asTracking, cancellationToken);
            if (workProfilePermissionsValidationResult.IsFailure)
                return Result<AssumeWorkProfileResponse>.Fail(workProfilePermissionsValidationResult.Errors);
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId, currentUserDeviceId, ipAddress, userAgent,
                request.WorkProfileId!.Value, utcNow, WorkProfileType.OnlyWorkProfile, _currentUserService.Jti!, _currentUserService.AccessTokenExpiration!.Value,
                _currentUserService.IsPersistent!.Value, asTracking: asTracking, cancellationToken: cancellationToken);
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, user.SecurityStamp, user.TokenVersion, currentUserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, request.WorkProfileId!.Value);
            var assumeWorkProfileUser = await _userQuery.GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(user.Id, userSession.Id, asTracking, cancellationToken);
            if (assumeWorkProfileUser is null)
            {
                _logger.LogError("AssumeWorkProfileCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, UserSessionId={UserSessionId}", 
                    user.Id, userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (assumeWorkProfileUser.AssumeWorkProfileUserWorkProfiles is null || assumeWorkProfileUser.AssumeWorkProfileUserWorkProfiles.Count == 0)
            {
                _logger.LogError("AssumeWorkProfileCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", user.Id, 
                    userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (assumeWorkProfileUser.AssumeWorkProfileUserSession is null)
            {
                _logger.LogError("AssumeWorkProfileCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", user.Id, userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            return Result<AssumeWorkProfileResponse>.Ok(new AssumeWorkProfileResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier, 
                refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt, assumeWorkProfileUser));
        }
    }
}