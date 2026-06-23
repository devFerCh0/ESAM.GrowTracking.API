using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile
{
    public class GetCurrentUserWorkProfileQueryHandler : IRequestHandler<GetCurrentUserWorkProfileQuery, Result<GetCurrentUserWorkProfileResponse>>
    {
        private readonly ILogger<GetCurrentUserWorkProfileQueryHandler> _logger;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserQuery _userQuery;

        public GetCurrentUserWorkProfileQueryHandler(ILogger<GetCurrentUserWorkProfileQueryHandler> logger, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService,
            IUserQuery userQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userQuery);
            _logger = logger;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userQuery = userQuery;
        }

        public async Task<Result<GetCurrentUserWorkProfileResponse>> Handle(GetCurrentUserWorkProfileQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
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
            if (currentUserWorkProfile.CurrentUserWorkProfileUserSession.CurrentUserWorkProfileSessionWorkProfileSelected is null)
            {
                _logger.LogError("GetCurrentUserWorkProfileQuery: perfil de trabajo ausente en los datos retornados. UserId={UserId}, UserSessionId={UserSessionId}",
                    currentUserId, currentUserSessionId);
                return Result<GetCurrentUserWorkProfileResponse>.Fail(Error.ServerError("No se encontró perfil de trabajo de usuario seleccionado."));
            }
            return Result<GetCurrentUserWorkProfileResponse>.Ok(currentUserWorkProfile);
        }
    }
}