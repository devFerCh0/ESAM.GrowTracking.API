using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
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
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IClientInfoService _clientInfoService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionService _userSessionService;
        private readonly IUserQuery _userQuery;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public AssumeWorkProfileCommandHandler(ILogger<AssumeWorkProfileCommandHandler> logger, IValidator<AssumeWorkProfileCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserWorkProfileRepository userWorkProfileRepository, 
            IWorkProfilePermissionRepository workProfilePermissionRepository, IClientInfoService clientInfoService, IDateTimeService dateTimeService, 
            IUserSessionService userSessionService, IUserQuery userQuery, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(clientInfoService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(userQuery);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _clientInfoService = clientInfoService;
            _dateTimeService = dateTimeService;
            _userSessionService = userSessionService;
            _userQuery = userQuery;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
        }

        public async Task<Result<AssumeWorkProfileResponse>> Handle(AssumeWorkProfileCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<AssumeWorkProfileResponse>.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.OnlyWorkProfile,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, request.WorkProfileId);
                return Result<AssumeWorkProfileResponse>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            var workProfileHasActivePermissionsWithAccess = await _workProfilePermissionRepository.HasActivePermissionsWithAccessAsync(request.WorkProfileId, asTracking, 
                cancellationToken);
            if (!workProfileHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("AssumeWorkProfileCommand: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", request.WorkProfileId);
                return Result<AssumeWorkProfileResponse>.Fail(Error.Unauthorized("El perfil de trabajo no tiene permisos activos asignados."));
            }
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            var currentIsPersistent = _accessTokenClaimsValidatorService.CurrentIsPersistent;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId, currentUserDeviceId, ipAddress, userAgent, currentIsPersistent,
                request.WorkProfileId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            var assumeWorkProfileUser = await _userQuery.GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(currentUserId, userSession.Id, asTracking, cancellationToken);
            if (assumeWorkProfileUser is null)
            {
                _logger.LogError("AssumeWorkProfileCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (assumeWorkProfileUser.AssumeWorkProfileUserWorkProfiles is null || assumeWorkProfileUser.AssumeWorkProfileUserWorkProfiles.Count == 0)
            {
                _logger.LogError("AssumeWorkProfileCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (assumeWorkProfileUser.AssumeWorkProfileUserSession is null)
            {
                _logger.LogError("AssumeWorkProfileCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, userSession.Id);
                return Result<AssumeWorkProfileResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, request.WorkProfileId, WorkProfileType.OnlyWorkProfile);
            return Result<AssumeWorkProfileResponse>.Ok(new AssumeWorkProfileResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier,
                refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt, assumeWorkProfileUser));
        }
    }
}