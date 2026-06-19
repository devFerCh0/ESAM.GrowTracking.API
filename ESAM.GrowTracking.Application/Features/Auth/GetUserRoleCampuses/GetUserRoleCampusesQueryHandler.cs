using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
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
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IUserRoleCampusQuery _userRoleCampusQuery;

        public GetUserRoleCampusesQueryHandler(ILogger<GetUserRoleCampusesQueryHandler> logger, IValidator<GetUserRoleCampusesQuery> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserWorkProfileRepository userWorkProfileRepository, IUserRoleCampusQuery userRoleCampusQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
            ArgumentNullException.ThrowIfNull(userRoleCampusQuery);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userWorkProfileRepository = userWorkProfileRepository;
            _userRoleCampusQuery = userRoleCampusQuery;
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
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;

            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.WithRoles, 
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}", 
                    currentUserId, request.WorkProfileId);
                return Result<List<GetUserRoleCampusResponse>>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo especificado asignado al usuario."));
            }
            var userRoleCampuses = await _userRoleCampusQuery.GetUserRoleCampusesByUserIdAsync(currentUserId, asTracking, cancellationToken);
            if (userRoleCampuses is null || userRoleCampuses.Count == 0)
            {
                _logger.LogWarning("GetUserRoleCampusesQuery: no se encontraron roles de sede asignados al usuario. UserId={UserId}", currentUserId);
                return Result<List<GetUserRoleCampusResponse>>.Fail(Error.NotFound("No se encontraron roles asignados para el usuario."));
            }
            return Result<List<GetUserRoleCampusResponse>>.Ok(userRoleCampuses);
        }
    }
}