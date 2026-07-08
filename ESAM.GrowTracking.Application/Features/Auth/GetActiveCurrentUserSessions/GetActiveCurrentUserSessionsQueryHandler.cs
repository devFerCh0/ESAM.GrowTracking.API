using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions
{
    public class GetActiveCurrentUserSessionsQueryHandler : IRequestHandler<GetActiveCurrentUserSessionsQuery, Result<List<GetActiveCurrentUserSessionsResponse>>>
    {
        private readonly ILogger<GetActiveCurrentUserSessionsQueryHandler> _logger;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionQuery _userSessionQuery;
        private readonly IDateTimeService _dateTimeService;

        public GetActiveCurrentUserSessionsQueryHandler(ILogger<GetActiveCurrentUserSessionsQueryHandler> logger, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IUserSessionQuery userSessionQuery,
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userSessionQuery);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _userSessionQuery = userSessionQuery;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result<List<GetActiveCurrentUserSessionsResponse>>> Handle(GetActiveCurrentUserSessionsQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var utcNow = _dateTimeService.UtcNow;
            var activeCurrentUserSessions = await _userSessionQuery.GetActiveCurrentUserSessionsByUserIdAsync(currentUserId, currentUserSessionId, utcNow, asTracking, 
                cancellationToken);
            if (activeCurrentUserSessions.Count == 0)
            {
                _logger.LogWarning("GetActiveCurrentUserSessionsQuery: no se encontraron sesiones activas. UserId={UserId}", currentUserId);
                return Result<List<GetActiveCurrentUserSessionsResponse>>.Fail(Error.NotFound("No se encontraron sesiones activas para el usuario."));
            }
            return Result<List<GetActiveCurrentUserSessionsResponse>>.Ok(activeCurrentUserSessions);
        }
    }
}