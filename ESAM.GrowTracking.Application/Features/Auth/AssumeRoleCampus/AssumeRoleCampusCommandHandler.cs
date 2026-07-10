using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
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
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IClientInfoService _clientInfoService;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserQuery _userQuery;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public AssumeRoleCampusCommandHandler(ILogger<AssumeRoleCampusCommandHandler> logger, IValidator<AssumeRoleCampusCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserWorkProfileRepository userWorkProfileRepository, 
            IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository, IClientInfoService clientInfoService,
            IUserSessionService userSessionService, IDateTimeService dateTimeService, IUserQuery userQuery, ITokenService tokenService, 
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            ArgumentNullException.ThrowIfNull(clientInfoService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userQuery);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userWorkProfileRepository = userWorkProfileRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _clientInfoService = clientInfoService;
            _userSessionService = userSessionService;
            _dateTimeService = dateTimeService;
            _userQuery = userQuery;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
        }

        public async Task<Result<AssumeRoleCampusResponse>> Handle(AssumeRoleCampusCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<AssumeRoleCampusResponse>.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.WithRoles,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, request.WorkProfileId);
                return Result<AssumeRoleCampusResponse>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, request.RoleId, request.CampusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: rol de sede de usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}", 
                    currentUserId, request.RoleId, request.CampusId);
                return Result<AssumeRoleCampusResponse>.Fail(Error.Unauthorized("No se encontró un rol de sede de usuario activo."));
            }
            var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(request.RoleId, asTracking, cancellationToken);
            if (!roleHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("AssumeRoleCampusCommand: rol sin permisos activos. RoleId={RoleId}", request.RoleId);
                return Result<AssumeRoleCampusResponse>.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
            }
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            var currentIsPersistent = _accessTokenClaimsValidatorService.CurrentIsPersistent;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var (refreshToken, userSession, userSessionWorkProfileSelectedId, userSessionRoleCampusSelectedId) = await _userSessionService.CreateUserSessionAsync(currentUserId,
                currentUserDeviceId, ipAddress, userAgent, currentIsPersistent, request.WorkProfileId, request.RoleId, request.CampusId, currentJti, currentAccessTokenExpiration, 
                utcNow, asTracking, cancellationToken);
            //var (refreshToken, userSession) = await _userSessionService.CreateUserSessionAsync(currentUserId, currentUserDeviceId, ipAddress, userAgent, currentIsPersistent, 
            //    request.WorkProfileId, request.RoleId, request.CampusId, currentJti, currentAccessTokenExpiration, utcNow, asTracking, cancellationToken);
            var assumeRoleCampusUser = await _userQuery.GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(currentUserId, userSession.Id, asTracking, cancellationToken);
            if (assumeRoleCampusUser is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, " +
                    "UserSessionId={UserSessionId}", currentUserId, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles is null || assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles.Count == 0)
            {
                _logger.LogError("AssumeRoleCampusCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses is null || assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses.Count == 0)
            {
                _logger.LogError("AssumeRoleCampusCommand: el usuario no tiene roles de sede asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene roles de sede asignados."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserSession is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserSession.AssumeRoleCampusSessionWorkProfileSelected is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: perfil de trabajo ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("No se encontró perfil de trabajo de usuario seleccionado."));
            }
            if (assumeRoleCampusUser.AssumeRoleCampusUserSession.AssumeRoleCampusSessionWorkProfileSelected.AssumeRoleCampusSessionRoleCampusSelected is null)
            {
                _logger.LogError("AssumeRoleCampusCommand: rol y sede ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, userSession.Id);
                return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("No se encontró rol y sede de usuario seleccionado."));
            }
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, userSessionWorkProfileSelectedId, request.WorkProfileId, WorkProfileType.WithRoles, 
                userSessionRoleCampusSelectedId, request.RoleId, request.CampusId);
            //var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, userSession.Id, utcNow,
            //    _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, request.WorkProfileId, WorkProfileType.WithRoles, request.RoleId, request.CampusId);
            return Result<AssumeRoleCampusResponse>.Ok(new AssumeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier,
                refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt, assumeRoleCampusUser));
        }
    }
}