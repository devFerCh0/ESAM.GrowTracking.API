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

namespace ESAM.GrowTracking.Application.Features.Auth.GetChangeUserRoleCampuses
{
    public class GetChangeUserRoleCampusesQueryHandler : IRequestHandler<GetChangeUserRoleCampusesQuery, Result<List<GetChangeUserRoleCampusResponse>>>
    {
        private readonly ILogger<GetChangeUserRoleCampusesQueryHandler> _logger;
        private readonly IValidator<GetChangeUserRoleCampusesQuery> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserWorkProfileRepository _userWorkProfileRepository;
        private readonly IUserRoleCampusQuery _userRoleCampusQuery;

        public GetChangeUserRoleCampusesQueryHandler(ILogger<GetChangeUserRoleCampusesQueryHandler> logger, IValidator<GetChangeUserRoleCampusesQuery> validator,
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

        public async Task<Result<List<GetChangeUserRoleCampusResponse>>> Handle(GetChangeUserRoleCampusesQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetChangeUserRoleCampusesQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<GetChangeUserRoleCampusResponse>>.Fail(validation.ToCommandErrors());
            }
            var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
            if (currentWorkProfileId == request.WorkProfileId)
            {
                _logger.LogWarning("ChangeWorkProfileCommand: Está seleccionando el mismo perfil de trabajo. WorkProfileId={WorkProfileId}", request.WorkProfileId);
                return Result<List<GetChangeUserRoleCampusResponse>>.Fail(Error.Validation("Está seleccionando el mismo perfil de trabajo."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var isUserWorkProfileActiveAndOfType = await _userWorkProfileRepository.IsActiveAndOfTypeAsync(currentUserId, request.WorkProfileId, WorkProfileType.WithRoles,
                asTracking, cancellationToken);
            if (!isUserWorkProfileActiveAndOfType)
            {
                _logger.LogWarning("GetChangeUserRoleCampusesQuery: perfil de trabajo del usuario no encontrado o eliminado. UserId={UserId}, WorkProfileId={WorkProfileId}",
                    currentUserId, request.WorkProfileId);
                return Result<List<GetChangeUserRoleCampusResponse>>.Fail(Error.Unauthorized("No se encontró un perfil de trabajo activo del tipo " + 
                    "especificado asignado al usuario."));
            }
            var changeUserRoleCampuses = await _userRoleCampusQuery.GetChangeUserRoleCampusesByUserIdAsync(currentUserId, asTracking, cancellationToken);
            if (changeUserRoleCampuses is null || changeUserRoleCampuses.Count == 0)
            {
                _logger.LogWarning("GetChangeUserRoleCampusesQuery: no se encontraron roles de sede asignados al usuario. UserId={UserId}", currentUserId);
                return Result<List<GetChangeUserRoleCampusResponse>>.Fail(Error.NotFound("No se encontraron roles asignados para el usuario."));
            }
            return Result<List<GetChangeUserRoleCampusResponse>>.Ok(changeUserRoleCampuses);
        }
    }
}