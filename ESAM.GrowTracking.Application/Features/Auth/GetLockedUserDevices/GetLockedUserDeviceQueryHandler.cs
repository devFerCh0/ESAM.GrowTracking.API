using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices
{
    public class GetLockedUserDeviceQueryHandler : IRequestHandler<GetLockedUserDeviceQuery, Result<List<GetLockedUserDeviceResponse>>>
    {
        private readonly ILogger<GetLockedUserDeviceQueryHandler> _logger;
        private readonly IValidator<GetLockedUserDeviceQuery> _validator;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserDeviceQuery _userDeviceQuery;

        public GetLockedUserDeviceQueryHandler(ILogger<GetLockedUserDeviceQueryHandler> logger, IValidator<GetLockedUserDeviceQuery> validator, IDateTimeService dateTimeService,
            IUserDeviceQuery userDeviceQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userDeviceQuery);
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _userDeviceQuery = userDeviceQuery;
        }

        public async Task<Result<List<GetLockedUserDeviceResponse>>> Handle(GetLockedUserDeviceQuery request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("GetLockedUserDeviceQuery: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<List<GetLockedUserDeviceResponse>>.Fail(validation.ToCommandErrors());
            }
            var utcNow = _dateTimeService.UtcNow;
            var lockedUserDevices = await _userDeviceQuery.GetAllLockedByUserIdAsync(request.UserId, utcNow, asTracking, cancellationToken);
            if (lockedUserDevices is null || lockedUserDevices.Count == 0)
            {
                _logger.LogInformation("GetLockedUserDeviceQuery: no se encontraron dispositivos bloqueados actualmente.");
                return Result<List<GetLockedUserDeviceResponse>>.Fail(Error.NotFound("No hay actualmente dispositivos bloqueados."));
            }
            return Result<List<GetLockedUserDeviceResponse>>.Ok(lockedUserDevices);
        }
    }
}