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
            var utcNow =_dateTimeService.UtcNow;




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


            ////var asTracking = false;
            ////var validation = await _validator.ValidateAsync(request, cancellationToken);
            ////if (!validation.IsValid)
            ////{
            ////    _logger.LogWarning("GetUserRoleCampusesQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
            ////    return Result<List<GetUserRoleCampusResponse>>.Fail(validation.ToDomainErrors());
            ////}
            ////var utcNow = _dateTimeService.UtcNow;

            ////var workProfileTypeValidationResult = await _currentUserValidatorService.ValidateUserWorkProfileAndTypeAsync(currentUserId, request.WorkProfileId!.Value,
            ////    WorkProfileType.WithRoles, asTracking, cancellationToken);
            ////if (workProfileTypeValidationResult.IsFailure)
            ////    return Result<List<GetUserRoleCampusResponse>>.Fail(workProfileTypeValidationResult.Errors);
            ////var userRoleCampuses = await _userRoleCampusQuery.GetUserRoleCampusesByUserIdAsync(currentUserId, asTracking, cancellationToken);
            ////if (userRoleCampuses is null || userRoleCampuses.Count == 0)
            ////{
            ////    _logger.LogWarning("GetUserRoleCampusesQuery: no se encontraron roles de sede asignados al usuario. UserId={UserId}", currentUserId);
            ////    return Result<List<GetUserRoleCampusResponse>>.Fail(Error.NotFound("No se encontraron roles asignados para el usuario."));
            ////}
            ////return Result<List<GetUserRoleCampusResponse>>.Ok(userRoleCampuses);
        }
    }
}