using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions
{
    public class GetActiveUserSessionsQueryHandler : IRequestHandler<GetActiveUserSessionsQuery, Result<List<GetActiveUserSessionsResponse>>>
    {
        private readonly ILogger<GetActiveUserSessionsQueryHandler> _logger;
        private readonly IValidator<GetActiveUserSessionsQuery> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionQuery _userSessionQuery;
        private readonly IDateTimeService _dateTimeService;

        public GetActiveUserSessionsQueryHandler(ILogger<GetActiveUserSessionsQueryHandler> logger, IValidator<GetActiveUserSessionsQuery> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IUserSessionQuery userSessionQuery,
            IDateTimeService dateTimeService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userSessionQuery);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _userSessionQuery = userSessionQuery;
            _dateTimeService = dateTimeService;
        }

        public async Task<Result<List<GetActiveUserSessionsResponse>>> Handle(GetActiveUserSessionsQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetActiveUserSessionsQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<GetActiveUserSessionsResponse>>.Fail(validation.ToCommandErrors());
            }
            if (!await _userRepository.ExistsAsync(request.UserId, asTracking, cancellationToken))
            {
                _logger.LogWarning("GetActiveUserSessionsQuery: usuario seleccionado no encontrado. UserId={UserId}", request.UserId);
                return Result<List<GetActiveUserSessionsResponse>>.Fail(Error.NotFound("El usuario seleccionado no fue encontrado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            var activeUserSessions = await _userSessionQuery.GetActiveUserSessionsByUserIdAsync(request.UserId, utcNow, asTracking, cancellationToken);
            if (activeUserSessions.Count == 0)
            {
                _logger.LogWarning("GetActiveUserSessionsQuery: no se encontraron sesiones activas. UserId={UserId}", request.UserId);
                return Result<List<GetActiveUserSessionsResponse>>.Fail(Error.NotFound("No se encontraron sesiones activas para el usuario."));
            }
            return Result<List<GetActiveUserSessionsResponse>>.Ok(activeUserSessions);
        }
    }
}