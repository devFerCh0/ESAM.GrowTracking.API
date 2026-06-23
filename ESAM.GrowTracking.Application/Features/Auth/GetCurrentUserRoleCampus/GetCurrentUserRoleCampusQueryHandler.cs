using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus
{
    public class GetCurrentUserRoleCampusQueryHandler : IRequestHandler<GetCurrentUserRoleCampusQuery, Result<GetCurrentUserRoleCampusResponse>>
    {
        private readonly ILogger<GetCurrentUserRoleCampusQueryHandler> _logger;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserQuery _userQuery;

        public GetCurrentUserRoleCampusQueryHandler(ILogger<GetCurrentUserRoleCampusQueryHandler> logger, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, 
            IUserQuery userQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userQuery);
            _logger = logger;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userQuery = userQuery;
        }

        public async Task<Result<GetCurrentUserRoleCampusResponse>> Handle(GetCurrentUserRoleCampusQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
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
                _logger.LogError("GetCurrentUserRoleCampusResponse: sesión de usuario ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", 
                    currentUserId, currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("No se encontró la sesión del usuario."));
            }
            if (currentUserRoleCampus.CurrentUserRoleCampusUserSession.CurrentUserRoleCampusSessionWorkProfileSelected is null)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: perfil de trabajo ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}", 
                    currentUserId, currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("No se encontró perfil de trabajo de usuario seleccionado."));
            }
            if (currentUserRoleCampus.CurrentUserRoleCampusUserSession.CurrentUserRoleCampusSessionWorkProfileSelected.CurrentUserRoleCampusSessionRoleCampusSelected is null)
            {
                _logger.LogError("GetCurrentUserRoleCampusResponse: rol y sede ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, currentUserSessionId);
                return Result<GetCurrentUserRoleCampusResponse>.Fail(Error.ServerError("No se encontró rol y sede de usuario seleccionado."));
            }
            return Result<GetCurrentUserRoleCampusResponse>.Ok(currentUserRoleCampus);
        }
    }
}