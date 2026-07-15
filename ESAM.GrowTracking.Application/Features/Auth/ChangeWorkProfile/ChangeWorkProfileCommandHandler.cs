using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile
{
    public class ChangeWorkProfileCommandHandler : IRequestHandler<ChangeWorkProfileCommand, Result<ChangeWorkProfileResponse>>
    {
        private readonly ILogger<ChangeWorkProfileCommandHandler> _logger;
        private readonly IValidator<ChangeWorkProfileCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserQuery _userQuery;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public ChangeWorkProfileCommandHandler(ILogger<ChangeWorkProfileCommandHandler> logger, IValidator<ChangeWorkProfileCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserWorkProfileRepository userWorkProfileRepository,
            IWorkProfilePermissionRepository workProfilePermissionRepository, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, 
            IDateTimeService dateTimeService, IUserQuery userQuery, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userQuery);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userWorkProfileRepository = userWorkProfileRepository;
            _workProfilePermissionRepository = workProfilePermissionRepository;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _dateTimeService = dateTimeService;
            _userQuery = userQuery;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
        }

        public async Task<Result<ChangeWorkProfileResponse>> Handle(ChangeWorkProfileCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("ChangeWorkProfileCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<ChangeWorkProfileResponse>.Fail(validation.ToCommandErrors());
            }
            var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
            if (currentWorkProfileId == request.WorkProfileId)
            {
                _logger.LogWarning("ChangeWorkProfileCommand: Está seleccionando el mismo perfil de trabajo. WorkProfileId={WorkProfileId}", request.WorkProfileId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.Validation("Está seleccionando el mismo perfil de trabajo."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.OnlyWorkProfile,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("ChangeWorkProfileCommand: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, request.WorkProfileId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            var workProfileHasActivePermissionsWithAccess = await _workProfilePermissionRepository.HasActivePermissionsWithAccessAsync(request.WorkProfileId, asTracking,
                cancellationToken);
            if (!workProfileHasActivePermissionsWithAccess)
            {
                _logger.LogWarning("ChangeWorkProfileCommand: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", request.WorkProfileId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.Unauthorized("El perfil de trabajo no tiene permisos activos asignados."));
            }
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var userSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
            var currentWorkProfileSelectedId = _accessTokenClaimsValidatorService.CurrentWorkProfileSelectedId;
            var currentRoleCampusSelectedId = _accessTokenClaimsValidatorService.CurrentRoleCampusSelectedId;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var workProfileSelectedId = await _userSessionService.ChangeWorkProfileAsync(userSession!, request.WorkProfileId, currentWorkProfileSelectedId,
                currentRoleCampusSelectedId, currentJti, currentAccessTokenExpiration, currentUserId, "Cambio de perfil de trabajo: Access token de sesión anterior revocado.", 
                utcNow, asTracking, cancellationToken);
            var changeWorkProfileUser = await _userQuery.GetChangeWorkProfileUserByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, cancellationToken);
            if (changeWorkProfileUser is null)
            {
                _logger.LogError("ChangeWorkProfileCommand: información del usuario no encontrada tras creación de sesión. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, currentUserSessionId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (changeWorkProfileUser.ChangeWorkProfileUserWorkProfiles is null || changeWorkProfileUser.ChangeWorkProfileUserWorkProfiles.Count == 0)
            {
                _logger.LogError("ChangeWorkProfileCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    currentUserSessionId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (changeWorkProfileUser.ChangeWorkProfileUserSession is null)
            {
                _logger.LogError("ChangeWorkProfileCommand: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            if (changeWorkProfileUser.ChangeWorkProfileUserSession.ChangeWorkProfileSessionWorkProfileSelected is null)
            {
                _logger.LogError("ChangeWorkProfileCommand: perfil de trabajo ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                    currentUserSessionId);
                return Result<ChangeWorkProfileResponse>.Fail(Error.ServerError("No se encontró perfil de trabajo de usuario seleccionado."));
            }
            var currentSecurityStamp = _accessTokenClaimsValidatorService.CurrentSecurityStamp;
            var currentTokenVersion = _accessTokenClaimsValidatorService.CurrentTokenVersion;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var accessToken = _tokenService.GenerateSessionAccessToken(currentUserId, currentSecurityStamp, currentTokenVersion, currentUserDeviceId, currentUserSessionId, utcNow, 
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileSelectedId, request.WorkProfileId, WorkProfileType.OnlyWorkProfile);
            return Result<ChangeWorkProfileResponse>.Ok(new ChangeWorkProfileResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, changeWorkProfileUser));
        }
    }
}