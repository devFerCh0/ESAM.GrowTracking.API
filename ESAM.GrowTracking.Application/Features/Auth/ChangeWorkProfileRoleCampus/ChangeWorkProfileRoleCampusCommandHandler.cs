using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus
{
    public class ChangeWorkProfileRoleCampusCommandHandler : IRequestHandler<ChangeWorkProfileRoleCampusCommand, Result<ChangeWorkProfileRoleCampusResponse>>
    {
        private readonly ILogger<ChangeWorkProfileRoleCampusCommandHandler> _logger;
        private readonly IValidator<ChangeWorkProfileRoleCampusCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionService _userSessionService;
        private readonly IUserQuery _userQuery;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public ChangeWorkProfileRoleCampusCommandHandler(ILogger<ChangeWorkProfileRoleCampusCommandHandler> logger, IValidator<ChangeWorkProfileRoleCampusCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserWorkProfileRepository userWorkProfileRepository, 
            IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository, IUserSessionRepository userSessionRepository, 
            IDateTimeService dateTimeService, IUserSessionService userSessionService, IUserQuery userQuery, ITokenService tokenService, 
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(userQuery);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userWorkProfileRepository = userWorkProfileRepository;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userSessionRepository = userSessionRepository;
            _dateTimeService = dateTimeService;
            _userSessionService = userSessionService;
            _userQuery = userQuery;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
        }

        public async Task<Result<ChangeWorkProfileRoleCampusResponse>> Handle(ChangeWorkProfileRoleCampusCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("ChangeWorkProfileRoleCampusCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(validation.ToCommandErrors());
            }
            var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
            if (currentWorkProfileId == request.WorkProfileId)
            {
                _logger.LogWarning("ChangeWorkProfileRoleCampusCommand: Está seleccionando el mismo perfil de trabajo. WorkProfileId={WorkProfileId}", request.WorkProfileId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.Validation("Está seleccionando el mismo perfil de trabajo."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.WithRoles,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("ChangeWorkProfileRoleCampusCommand: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, request.WorkProfileId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo " + 
                    "del tipo especificado asignado al usuario."));
            }
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, request.RoleId, request.CampusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("ChangeWorkProfileRoleCampusCommand: rol de sede de usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
                    currentUserId, request.RoleId, request.CampusId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.Unauthorized("No se encontró un rol de sede de usuario activo."));
            }
            var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(request.RoleId, asTracking, cancellationToken);
            if (!roleHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ChangeWorkProfileRoleCampusCommand: rol sin permisos activos. RoleId={RoleId}", request.RoleId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
            }
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var userSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
            var currentWorkProfileSelectedId = _accessTokenClaimsValidatorService.CurrentWorkProfileSelectedId;
            var currentRoleCampusSelectedId = _accessTokenClaimsValidatorService.CurrentRoleCampusSelectedId;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var (workProfileSelectedId, roleCampusSelectedId) = await _userSessionService.ChangeWorkProfileRoleCampusAsync(userSession!, request.WorkProfileId, request.RoleId, 
                request.CampusId, currentWorkProfileSelectedId, currentRoleCampusSelectedId, currentJti, currentAccessTokenExpiration, currentUserId, 
                "Cambio de perfil de trabajo: Access token de sesión anterior revocado.", utcNow, asTracking, cancellationToken);
            var changeWorkProfileRoleCampusUser = await _userQuery.GetChangeWorkProfileRoleCampusUserByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, 
                cancellationToken);
            if (changeWorkProfileRoleCampusUser is null)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, " +
                    "UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserWorkProfiles is null || 
                changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserWorkProfiles.Count == 0)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", 
                    currentUserId, currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserRoleCampuses is null || 
                changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserRoleCampuses.Count == 0)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: el usuario no tiene roles de sede asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene roles de sede asignados."));
            }
            if (changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserSession is null)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            if (changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserSession.ChangeWorkProfileRoleCampusSessionWorkProfileSelected is null)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: perfil de trabajo ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", 
                    currentUserId, currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("No se encontró perfil de trabajo de usuario seleccionado."));
            }
            if (changeWorkProfileRoleCampusUser.ChangeWorkProfileRoleCampusUserSession.ChangeWorkProfileRoleCampusSessionWorkProfileSelected
                .ChangeWorkProfileRoleCampusSessionRoleCampusSelected is null)
            {
                _logger.LogError("ChangeWorkProfileRoleCampusCommand: rol y sede ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    currentUserSessionId);
                return Result<ChangeWorkProfileRoleCampusResponse>.Fail(Error.ServerError("No se encontró rol y sede de usuario seleccionado."));
            }
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileSelectedId, request.WorkProfileId, WorkProfileType.WithRoles, roleCampusSelectedId, 
                request.RoleId, request.CampusId);
            return Result<ChangeWorkProfileRoleCampusResponse>.Ok(new ChangeWorkProfileRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt,
                changeWorkProfileRoleCampusUser));
        }
    }
}