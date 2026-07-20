using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries.Filters;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses;
using ESAM.GrowTracking.Application.Results;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResponse<GetUsersResponse>>>
    {
        private readonly ILogger<GetUsersQueryHandler> _logger;
        private readonly IValidator<GetUsersQuery> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserQuery _userQuery;

        public GetUsersQueryHandler(ILogger<GetUsersQueryHandler> logger, IValidator<GetUsersQuery> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, IUserQuery userQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userQuery);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userQuery = userQuery;
        }

        public async Task<Result<PagedResponse<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUsersQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<PagedResponse<GetUsersResponse>>.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var normalizedSearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
            var filter = new GetUsersFilter(normalizedSearchTerm, request.IsDeleted, request.IsLocked, request.WorkProfileId, request.SortBy, request.SortDirection, 
                request.PageNumber, request.PageSize, utcNow);
            var pagedUsers = await _userQuery.GetUsersAsync(filter, asTracking, cancellationToken);
            if (pagedUsers.Items.Count == 0)
                _logger.LogInformation("GetUsersQuery: no se encontraron usuarios para los criterios especificados. AdminUserId={AdminUserId}, Página={PageNumber}, " +
                    "TamañoPágina={PageSize}", currentUserId, request.PageNumber, request.PageSize);
            else
                _logger.LogInformation("GetUsersQuery: listado obtenido exitosamente. AdminUserId={AdminUserId}, Página={PageNumber}, TamañoPágina={PageSize}, " +
                    "TotalRegistros={TotalCount}.", currentUserId, pagedUsers.PageNumber, pagedUsers.PageSize, pagedUsers.TotalCount);
            return Result<PagedResponse<GetUsersResponse>>.Ok(pagedUsers);
        }
    }
}