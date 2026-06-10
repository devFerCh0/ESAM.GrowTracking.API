using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile
{
    public class GetCurrentUserWorkProfileQueryHandler : IRequestHandler<GetCurrentUserWorkProfileQuery, Result<GetCurrentUserWorkProfileResponse>>
    {
        private readonly ILogger<GetCurrentUserWorkProfileQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserQuery _userQuery;

        public GetCurrentUserWorkProfileQueryHandler(ILogger<GetCurrentUserWorkProfileQueryHandler> logger, ICurrentUserService currentUserService, 
            IDateTimeService dateTimeService, ICurrentUserValidatorService currentUserValidatorService, IUserQuery userQuery)
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

        public async Task<Result<GetCurrentUserWorkProfileResponse>> Handle(GetCurrentUserWorkProfileQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetCurrentUserWorkProfileQuery: intento de acceso no autenticado.");
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            if (_currentUserService.AccessTokenType != AccessTokenType.Session)
            {
                _logger.LogWarning("GetCurrentUserWorkProfileQuery: tipo de token inválido. Esperado=Session, Actual={AccessTokenType}", _currentUserService.AccessTokenType);
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso de sesión."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var currentUserSessionId = _currentUserService.UserSessionId!.Value;
            var currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
            var utcNow = _dateTimeService.UtcNow;

            var validateCurrentUser = await _currentUserValidatorService.ValidateCurrentUserAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (validateCurrentUser.IsFailure)
                return Result<GetCurrentUserWorkProfileResponse>.Fail(validateCurrentUser.Errors);

            var validateCurrentUserDevice = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking, 
                cancellationToken);
            if (validateCurrentUserDevice.IsFailure)
                return Result<GetCurrentUserWorkProfileResponse>.Fail(validateCurrentUserDevice.Errors);

            // Validar Sesión

            var validateUserWorkProfileAndTypeAndHasPermissions = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAndHasPermissionsAsync(currentUserId, 
                currentWorkProfileId, WorkProfileType.OnlyWorkProfile, asTracking, cancellationToken);
            if (validateUserWorkProfileAndTypeAndHasPermissions.IsFailure)
                return Result<GetCurrentUserWorkProfileResponse>.Fail(validateUserWorkProfileAndTypeAndHasPermissions.Errors);

            var currentUserWorkProfile = await _userQuery.GetCurrentUserWorkProfileByUserIdAndUserSessionIdAsync(currentUserId, currentUserSessionId, asTracking, 
                cancellationToken);
            if (currentUserWorkProfile is null)
            {
                _logger.LogError("GetCurrentUserWorkProfileQuery: información del usuario no encontrada. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (currentUserWorkProfile.CurrentUserWorkProfileUserWorkProfiles is null || currentUserWorkProfile.CurrentUserWorkProfileUserWorkProfiles.Count == 0)
            {
                _logger.LogError("GetCurrentUserWorkProfileQuery: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.ServerError("El usuario no tiene perfiles de trabajo asignados."));
            }
            if (currentUserWorkProfile.CurrentUserWorkProfileUserSession is null)
            {
                _logger.LogError("GetCurrentUserWorkProfileQuery: sesión ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
                    currentUserSessionId);
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }

            return Result<GetCurrentUserWorkProfileResponse>.Ok(currentUserWorkProfile);
        }
    }
}