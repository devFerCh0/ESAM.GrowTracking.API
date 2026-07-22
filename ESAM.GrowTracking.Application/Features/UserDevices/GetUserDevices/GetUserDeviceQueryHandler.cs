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

namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public class GetUserDevicesQueryHandler : IRequestHandler<GetUserDevicesQuery, Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>>
    {
        private readonly ILogger<GetUserDevicesQueryHandler> _logger;
        private readonly IValidator<GetUserDevicesQuery> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserDeviceQuery _userDeviceQuery;

        public GetUserDevicesQueryHandler(ILogger<GetUserDevicesQueryHandler> logger, IValidator<GetUserDevicesQuery> validator, IUserRepository userRepository,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService, IUserDeviceQuery userDeviceQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userDeviceQuery);
            _logger = logger;
            _validator = validator;
            _userRepository = userRepository;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userDeviceQuery = userDeviceQuery;
        }

        public async Task<Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>> Handle(GetUserDevicesQuery request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetUserDevicesQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var isUserValid = await _userRepository.IsUserValidAsync(request.UserId, asTracking, cancellationToken);
            if (!isUserValid)
            {
                _logger.LogWarning("GetUserDevicesQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var normalizedSearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
            var userDevicesFilter = new GetUserDevicesFilter(request.UserId, request.IsLocked, request.ApiClientType,  request.IsDeleted, normalizedSearchTerm, 
                request.UserDevicesSortBy, request.SortDirection, request.PageNumber, request.PageSize, utcNow);
            var userDevicesPaged = await _userDeviceQuery.GetUserDevicesAsync(userDevicesFilter, asTracking, cancellationToken);
            return Result<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>>.Ok(userDevicesPaged);
        }
    }
}