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
    public class GetUserRoleCampusesQueryHandler : IRequestHandler<GetUserRoleCampusesQuery, Result<List<GetUserRoleCampusResponse>>>
    {
        private readonly ILogger<GetUserRoleCampusesQueryHandler> _logger;
        private readonly IValidator<GetUserRoleCampusesQuery> _validator;
        private readonly ITokenClaimsValidationService _tokenClaimsValidationService;
        private readonly ICurrentSessionValidationService _currentSessionValidationService;
        private readonly IDateTimeService _dateTimeService;
        //private readonly ICurrentUserService _currentUserService;
        //private readonly ICurrentUserValidatorService _currentUserValidatorService;
        //private readonly IUserRoleCampusQuery _userRoleCampusQuery;

        public GetUserRoleCampusesQueryHandler(ILogger<GetUserRoleCampusesQueryHandler> logger, IValidator<GetUserRoleCampusesQuery> validator, 
            ITokenClaimsValidationService tokenClaimsValidationService, ICurrentSessionValidationService currentSessionValidationService, IDateTimeService dateTimeService,

            ICurrentUserService currentUserService, ICurrentUserValidatorService currentUserValidatorService, 
            IUserRoleCampusQuery userRoleCampusQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(tokenClaimsValidationService);
            ArgumentNullException.ThrowIfNull(currentSessionValidationService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            //ArgumentNullException.ThrowIfNull(currentUserService);
            //ArgumentNullException.ThrowIfNull(currentUserValidatorService);
            //ArgumentNullException.ThrowIfNull(userRoleCampusQuery);
            _logger = logger;
            _validator = validator;
            _tokenClaimsValidationService = tokenClaimsValidationService;
            _currentSessionValidationService = currentSessionValidationService;
            _dateTimeService = dateTimeService;
            //_currentUserService = currentUserService;
            //_currentUserValidatorService = currentUserValidatorService;
            //_userRoleCampusQuery = userRoleCampusQuery;
        }

        public async Task<Result<List<GetUserRoleCampusResponse>>> Handle(GetUserRoleCampusesQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<GetUserRoleCampusResponse>>.Fail(validation.ToCommandErrors());
            }
            if (!_tokenClaimsValidationService.IsAuthenticated)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: intento de acceso no autenticado.");
                return Result<List<GetUserRoleCampusResponse>>.Fail(Error.Unauthorized("Sesión inválida o expirada. Inicie sesión nuevamente."));
            }
            var currentAccessTokenType = _tokenClaimsValidationService.CurrentAccessTokenType;
            if (currentAccessTokenType != AccessTokenType.Temporary)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: tipo de token de acceso inválido. Esperado=Temporal, Actual={AccessTokenType}", currentAccessTokenType);
                return Result<List<GetUserRoleCampusResponse>>.Fail(Error.Unauthorized("Esta operación requiere un token de acceso temporal."));
            }
            var currentUserId = _tokenClaimsValidationService.CurrentUserId;
            var currentSecurityStamp = _tokenClaimsValidationService.CurrentSecurityStamp;
            var currentTokenVersion = _tokenClaimsValidationService.CurrentTokenVersion;
            var utcNow =_dateTimeService.UtcNow;
            var validateCurrentUserResult = await _currentSessionValidationService.ValidateCurrentUserAsync(currentUserId, currentSecurityStamp, currentTokenVersion, utcNow, 
                asTracking,
                cancellationToken);
            if (validateCurrentUserResult.IsFailure)
                return Result<List<GetUserRoleCampusResponse>>.Fail(validateCurrentUserResult.Errors);
            var currentUserDeviceId = _tokenClaimsValidationService.CurrentUserDeviceId;
            var validateUserDeviceResult = await _currentSessionValidationService.ValidateCurrentUserDeviceAsync(currentUserDeviceId, currentUserId, utcNow, asTracking,
                cancellationToken);
            if (validateUserDeviceResult.IsFailure)
                return Result<List<GetUserRoleCampusResponse>>.Fail(validateUserDeviceResult.Errors);


            
            //var workProfileTypeValidationResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId, request.WorkProfileId!.Value, 
            //    WorkProfileType.WithRoles, asTracking, cancellationToken);
            //if (workProfileTypeValidationResult.IsFailure)
            //    return Result<List<GetUserRoleCampusResponse>>.Fail(workProfileTypeValidationResult.Errors);
            //var userRoleCampuses = await _userRoleCampusQuery.GetUserRoleCampusesByUserIdAsync(currentUserId, asTracking, cancellationToken);
            //if (userRoleCampuses is null || userRoleCampuses.Count == 0)
            //{
            //    _logger.LogWarning("GetUserRoleCampusesQuery: no se encontraron roles de sede asignados al usuario. UserId={UserId}", currentUserId);
            //    return Result<List<GetUserRoleCampusResponse>>.Fail(Error.NotFound("No se encontraron roles asignados para el usuario."));
            //}
            //return Result<List<GetUserRoleCampusResponse>>.Ok(userRoleCampuses);
        }
    }
}