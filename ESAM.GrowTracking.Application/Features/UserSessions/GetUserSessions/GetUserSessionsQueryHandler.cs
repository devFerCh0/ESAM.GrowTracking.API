using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>>
    {
        private readonly ILogger<GetUserSessionsQueryHandler> _logger;
        private readonly IValidator<GetUserSessionsQuery> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionQuery _userSessionQuery;

        public GetUserSessionsQueryHandler(ILogger<GetUserSessionsQueryHandler> logger, IValidator<GetUserSessionsQuery> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IDateTimeService dateTimeService, 
            IUserSessionQuery userSessionQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionQuery);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _userSessionQuery = userSessionQuery;
        }

        public async Task<Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUserSessionsQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var isUserValid = await _userRepository.IsUserValidAsync(request.UserId, asTracking, cancellationToken);
            if (!isUserValid)
            {
                _logger.LogWarning("GetUserSessionsQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var normalizedSearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
            var userSessionsFilter = new GetUserSessionsFilter(request.UserId, request.IsActive, request.ApiClientType, normalizedSearchTerm, request.UserSessionsSortBy,
               request.SortDirection, request.PageNumber, request.PageSize, utcNow);
            var userSessionsPaged = await _userSessionQuery.GetUserSessionsAsync(userSessionsFilter, asTracking, cancellationToken);
            return Result<PagedResponse<GetUserSessionsResponse.UserSessionResponse>>.Ok(userSessionsPaged);
        }
    }
}