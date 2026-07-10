using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Services;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus
{
    public class ChangeRoleCampusCommandHandler : IRequestHandler<ChangeRoleCampusCommand, Result<ChangeRoleCampusResponse>>
    {
        private readonly ILogger<ChangeRoleCampusCommandHandler> _logger;
        private readonly IValidator<ChangeRoleCampusCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRoleCampusRepository _userRoleCampusRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;

        public ChangeRoleCampusCommandHandler(ILogger<ChangeRoleCampusCommandHandler> logger, IValidator<ChangeRoleCampusCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRoleCampusRepository userRoleCampusRepository, 
            IRolePermissionRepository rolePermissionRepository, IUserSessionService userSessionService, IUserSessionRepository userSessionRepository, 
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
            ArgumentNullException.ThrowIfNull(rolePermissionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRoleCampusRepository = userRoleCampusRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userSessionService = userSessionService;
            _userSessionRepository = userSessionRepository;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result<ChangeRoleCampusResponse>> Handle(ChangeRoleCampusCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("ChangeRoleCampusCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<ChangeRoleCampusResponse>.Fail(validation.ToCommandErrors());
            }
            var currentRoleId = _accessTokenClaimsValidatorService.CurrentRoleId;
            var currentCampusId = _accessTokenClaimsValidatorService.CurrentCampusId;
            if (currentRoleId == request.RoleId && currentCampusId == request.CampusId)
            {
                _logger.LogWarning("ChangeRoleCampusCommand: Está seleccionando el mismo rol de sede. RoleId={RoleId}, CampusId={CampusId}", request.RoleId, request.CampusId);
                return Result<ChangeRoleCampusResponse>.Fail(Error.Validation("Está seleccionando el mismo rol de sede."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserRoleCampusActive = await _userRoleCampusRepository.IsActiveAsync(currentUserId, request.RoleId, request.CampusId, asTracking, cancellationToken);
            if (!isUserRoleCampusActive)
            {
                _logger.LogWarning("ChangeRoleCampusCommand: rol de sede de usuario no encontrado o eliminado. UserId={UserId}, RoleId={RoleId}, CampusId={CampusId}",
                    currentUserId, request.RoleId, request.CampusId);
                return Result<ChangeRoleCampusResponse>.Fail(Error.Unauthorized("No se encontró un rol de sede de usuario activo."));
            }
            var roleHasActivePermissionsWithAccess = await _rolePermissionRepository.HasActivePermissionsWithAccessAsync(request.RoleId, asTracking, cancellationToken);
            if (!roleHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ChangeRoleCampusCommand: rol sin permisos activos. RoleId={RoleId}", request.RoleId);
                return Result<ChangeRoleCampusResponse>.Fail(Error.Unauthorized("El rol no tiene permisos activos asignados."));
            }
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var userSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                cancellationToken);
            var currentWorkProfileSelectedId = _accessTokenClaimsValidatorService.CurrentWorkProfileSelectedId;
            var currentRoleCampusSelectedId = _accessTokenClaimsValidatorService.CurrentRoleCampusSelectedId;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var roleCampusSelectedId = await _userSessionService.ChangeRoleCampusAsync(userSession!, currentWorkProfileSelectedId, currentRoleCampusSelectedId, request.RoleId, 
                request.CampusId, currentJti, currentAccessTokenExpiration, "Cambio de rol de sede: Access token de sesión anterior revocado.", currentUserId, utcNow, asTracking, 
                cancellationToken);



            //var changeRoleCampusUser = await _userQuery.GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, cancellationToken);
            //if (changeRoleCampusUser is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: información del usuario no encontrada tras el cambio de rol. UserId={UserId}, UserSessionId={UserSessionId}",
            //        currentUserId, currentUserSessionId);
            //    return Result<ChangeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            //}
            //if (changeRoleCampusUser.AssumeRoleCampusUserSession?.AssumeRoleCampusSessionWorkProfileSelected?.AssumeRoleCampusSessionRoleCampusSelected is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: rol y sede ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
            //        currentUserSessionId);
            //    return Result<ChangeRoleCampusResponse>.Fail(Error.ServerError("No se encontró rol y sede de usuario seleccionado."));
            //}
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentUserSessionWorkProfileSelectedId, currentWorkProfileId, WorkProfileType.WithRoles,
                newUserSessionRoleCampusSelectedId, request.RoleId, request.CampusId);
            return Result<ChangeRoleCampusResponse>.Ok(new ChangeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, changeRoleCampusUser));



            //var changeRoleCampusUser = await _userQuery.GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking,
            //    cancellationToken);
            //if (changeRoleCampusUser is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: información del usuario no encontrada tras cambio de rol de sede. UserId={UserId}, UserSessionId={UserSessionId}",
            //        currentUserId, currentUserSessionId);
            //    return Result<ChangeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            //}
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId,
                utcNow, _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentWorkProfileSelectedId, currentWorkProfileId, WorkProfileType.WithRoles,
                newRoleCampusSelectedId, request.RoleId, request.CampusId);
            _logger.LogInformation("ChangeRoleCampusCommand: cambio de rol de sede exitoso. UserId={UserId}, UserSessionId={UserSessionId}, RoleId={RoleId}, " +
                "CampusId={CampusId}", currentUserId, currentUserSessionId, request.RoleId, request.CampusId);
            return Result<ChangeRoleCampusResponse>.Ok(new ChangeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, changeRoleCampusUser));



            //var currentUser = await _userQuery.GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, cancellationToken);
            //if (currentUser is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: información del usuario no encontrada tras el cambio de rol. UserId={UserId}, UserSessionId={UserSessionId}",
            //        currentUserId, currentUserSessionId);
            //    return Result<ChangeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            //}
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId,
                utcNow, _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentUserSessionWorkProfileSelectedId, currentWorkProfileId, currentWorkProfileType,
                newUserSessionRoleCampusSelectedId, request.RoleId, request.CampusId);
            return Result<ChangeRoleCampusResponse>.Ok(new ChangeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, currentUser));



            //var assumeRoleCampusUser = await _userQuery.GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(currentUserId, newUserSession.Id, asTracking, cancellationToken);
            //if (assumeRoleCampusUser is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, UserSessionId={UserSessionId}",
            //        currentUserId, newUserSession.Id);
            //    return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            //}
            //if (assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles is null || assumeRoleCampusUser.AssumeRoleCampusUserWorkProfiles.Count == 0)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
            //        newUserSession.Id);
            //    return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            //}
            //if (assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses is null || assumeRoleCampusUser.AssumeRoleCampusUserRoleCampuses.Count == 0)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: el usuario no tiene roles de sede asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
            //        newUserSession.Id);
            //    return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene roles de sede asignados."));
            //}
            //if (assumeRoleCampusUser.AssumeRoleCampusUserSession is null ||
            //    assumeRoleCampusUser.AssumeRoleCampusUserSession.AssumeRoleCampusSessionWorkProfileSelected is null ||
            //    assumeRoleCampusUser.AssumeRoleCampusUserSession.AssumeRoleCampusSessionWorkProfileSelected.AssumeRoleCampusSessionRoleCampusSelected is null)
            //{
            //    _logger.LogError("ChangeRoleCampusCommand: datos de sesión incompletos en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
            //        newUserSession.Id);
            //    return Result<AssumeRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión, perfil o rol y sede de usuario seleccionado."));
            //}
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, newUserSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, userSessionWorkProfileSelectedId, currentWorkProfileId, WorkProfileType.WithRoles,
                userSessionRoleCampusSelectedId, request.RoleId, request.CampusId);
            return Result<AssumeRoleCampusResponse>.Ok(new AssumeRoleCampusResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier,
                refreshToken.Token, refreshToken.ExpiresIn, refreshToken.ExpiresAt, assumeRoleCampusUser));
        }
    }
}