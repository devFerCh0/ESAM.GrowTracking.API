using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus
{
    public class GetCurrentUserRoleCampusQueryHandler : IRequestHandler<GetCurrentUserRoleCampusQuery, Result<GetCurrentUserRoleCampusResponse>>
    {
        private readonly ILogger<GetCurrentUserRoleCampusQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserQuery _userQuery;

        public GetCurrentUserRoleCampusQueryHandler(ILogger<GetCurrentUserRoleCampusQueryHandler> logger, ICurrentUserService currentUserService, IDateTimeService dateTimeService, 
            ICurrentUserValidatorService currentUserValidatorService, IUserQuery userQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(currentUserValidatorService);
            ArgumentNullException.ThrowIfNull(userQuery);
            _logger = logger;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _currentUserValidatorService = currentUserValidatorService;
            _userQuery = userQuery;
        }

        public async Task<Result<GetCurrentUserRoleCampusResponse>> Handle(GetCurrentUserRoleCampusQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetCurrentUserRoleCampusResponse: intento de acceso no autenticado.");
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            if (_currentUserService.AccessTokenType != AccessTokenType.Session)
            {
                _logger.LogWarning("GetCurrentUserRoleCampusResponse: tipo de token inválido. Esperado=Session, Actual={AccessTokenType}", _currentUserService.AccessTokenType);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso de sesión."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var currentUserSessionId = _currentUserService.UserSessionId!.Value;
            var currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
            var currentRoleId = _currentUserService.RoleId!.Value;
            var currentCampusId = _currentUserService.CampusId!.Value;
            var utcNow = _dateTimeService.UtcNow;

            var validateCurrentUser = await _currentUserValidatorService.ValidateCurrentUserAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (validateCurrentUser.IsFailure)
                return Result<GetCurrentUserRoleCampusResponse>.Fail(validateCurrentUser.Errors);

            var validateCurrentUserDevice = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking,
                cancellationToken);
            if (validateCurrentUserDevice.IsFailure)
                return Result<GetCurrentUserRoleCampusResponse>.Fail(validateCurrentUserDevice.Errors);

            // Validar Sesión

            var validateUserWorkProfileAndType = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId, currentWorkProfileId, 
                WorkProfileType.WithRoles, asTracking, cancellationToken);
            if (validateUserWorkProfileAndType.IsFailure)
                return Result<GetCurrentUserRoleCampusResponse>.Fail(validateUserWorkProfileAndType.Errors);

            var validateUserRoleCampusAndHasPermissions = await _currentUserValidatorService.ValidateUserRoleCampusAndHasPermissionsAsync(currentUserId, currentRoleId, 
                currentCampusId, asTracking, cancellationToken);
            if (validateUserRoleCampusAndHasPermissions.IsFailure)
                return Result<GetCurrentUserRoleCampusResponse>.Fail(validateUserRoleCampusAndHasPermissions.Errors);

            var currentUserRoleCampus = await _userQuery.GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, cancellationToken);
            if (currentUserRoleCampus is null)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: información del usuario no encontrada. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (currentUserRoleCampus.CurrentUserRoleCampusUserWorkProfiles is null || currentUserRoleCampus.CurrentUserRoleCampusUserWorkProfiles.Count == 0)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", 
                    currentUserId, currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (currentUserRoleCampus.CurrentUserRoleCampusUserRoleCampuses is null || currentUserRoleCampus.CurrentUserRoleCampusUserRoleCampuses.Count == 0)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: el usuario no tiene roles de sede asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("El usuario no tiene roles de sede asignados."));
            }
            if (currentUserRoleCampus.CurrentUserRoleCampusUserSession is null)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }

            return Result<GetCurrentUserRoleCampusResponse>.Ok(currentUserRoleCampus);
        }
    }
}