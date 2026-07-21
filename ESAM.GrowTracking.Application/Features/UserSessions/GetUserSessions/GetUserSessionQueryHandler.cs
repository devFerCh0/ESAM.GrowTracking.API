using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public class GetUserSessionQueryHandler : IRequestHandler<GetUserSessionQuery, Result<PagedResponse<GetUserSessionResponse>>>
    {
        private readonly ILogger<GetUserSessionQueryHandler> _logger;
        private readonly IValidator<GetUserSessionQuery> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionQuery _userSessionQuery;

        public GetUserSessionQueryHandler(ILogger<GetUserSessionQueryHandler> logger, IValidator<GetUserSessionQuery> validator,
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

        public async Task<Result<PagedResponse<GetUserSessionResponse>>> Handle(GetUserSessionQuery request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUserSessionQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<PagedResponse<GetUserSessionResponse>>.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("GetUserSessionQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result<PagedResponse<GetUserSessionResponse>>.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var normalizedSearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
            var filter = new GetUserSessionFilter(request.UserId, request.IsActive, request.ApiClientType, normalizedSearchTerm, request.GetUserSessionSortBy,
               request.SortDirection, request.PageNumber, request.PageSize, utcNow);
            var userSessionsPaged = await _userSessionQuery.GetUserSessionsAsync(filter, asTracking, cancellationToken);
            return Result<PagedResponse<GetUserSessionResponse>>.Ok(userSessionsPaged);
        }
    }
}
