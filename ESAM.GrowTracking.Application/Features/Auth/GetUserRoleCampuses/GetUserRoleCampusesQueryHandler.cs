using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses
{
    public class GetUserRoleCampusesQueryHandler : IRequestHandler<GetUserRoleCampusesQuery, Result<List<UserRoleCampusResponse>>>
    {
        private readonly ILogger<GetUserRoleCampusesQueryHandler> _logger;
        private readonly IValidator<GetUserRoleCampusesQuery> _validator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserValidatorService _currentUserValidatorService;
        private readonly IUserRoleCampusQuery _userRoleCampusQuery;

        public GetUserRoleCampusesQueryHandler(ILogger<GetUserRoleCampusesQueryHandler> logger, IValidator<GetUserRoleCampusesQuery> validator, 
            ICurrentUserService currentUserService, IDateTimeService dateTimeService, ICurrentUserValidatorService currentUserValidatorService, 
            IUserRoleCampusQuery userRoleCampusQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(currentUserValidatorService);
            ArgumentNullException.ThrowIfNull(userRoleCampusQuery);
            _logger = logger;
            _validator = validator;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _currentUserValidatorService = currentUserValidatorService;
            _userRoleCampusQuery = userRoleCampusQuery;
        }

        public async Task<Result<List<UserRoleCampusResponse>>> Handle(GetUserRoleCampusesQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<UserRoleCampusResponse>>.Fail(validation.ToDomainErrors());
            }
            if (!_currentUserService.IsAuthenticated)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: intento de acceso no autenticado.");
                return Result<List<UserRoleCampusResponse>>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            if (_currentUserService.AccessTokenType != AccessTokenType.Temporary)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: tipo de token de acceso inválido. Esperado=Temporal, Actual={AccessTokenType}", _currentUserService.AccessTokenType);
                return Result<List<UserRoleCampusResponse>>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso temporal."));
            }
            var currentUserId = _currentUserService.UserId!.Value;
            var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
            var utcNow = _dateTimeService.UtcNow;
            var currentUserValidationResult = await _currentUserValidatorService.ValidateCurrentUserAsync(currentUserId, utcNow, asTracking, cancellationToken);
            if (currentUserValidationResult.IsFailure)
                return Result<List<UserRoleCampusResponse>>.Fail(currentUserValidationResult.Errors);
            var currentUserDeviceValidationResult = await _currentUserValidatorService.ValidateCurrentUserDeviceAsync(currentUserId, currentUserDeviceId, utcNow, asTracking, 
                cancellationToken);
            if (currentUserDeviceValidationResult.IsFailure)
                return Result<List<UserRoleCampusResponse>>.Fail(currentUserDeviceValidationResult.Errors);
            var workProfileTypeValidationResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId, request.WorkProfileId!.Value, 
                WorkProfileType.WithRoles, asTracking, cancellationToken);
            if (workProfileTypeValidationResult.IsFailure)
                return Result<List<UserRoleCampusResponse>>.Fail(workProfileTypeValidationResult.Errors);
            var userRoleCampuses = await _userRoleCampusQuery.GetUserRoleCampusesByUserIdAsync(currentUserId, asTracking, cancellationToken);
            if (userRoleCampuses is null || userRoleCampuses.Count == 0)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: no se encontraron roles de sede asignados al usuario. UserId={UserId}", currentUserId);
                return Result<List<UserRoleCampusResponse>>.Fail(Error.NotFound("No se encontraron roles asignados para el usuario."));
            }
            return Result<List<UserRoleCampusResponse>>.Ok(userRoleCampuses);
        }
    }
}