using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSessions
{
    public class CloseUserSessionsCommandHandler : IRequestHandler<CloseUserSessionsCommand, Result>
    {
        private readonly ILogger<CloseUserSessionsCommandHandler> _logger;
        private readonly IValidator<CloseUserSessionsCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserSessionRevocationService _userSessionRevocationService;
        private readonly IUnitOfWork _unitOfWork;

        public CloseUserSessionsCommandHandler(ILogger<CloseUserSessionsCommandHandler> logger, IValidator<CloseUserSessionsCommand> validator, IUserRepository userRepository,
            IDateTimeService dateTimeService, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserSessionRevocationService userSessionRevocationService,
            IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userSessionRevocationService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userSessionRevocationService = userSessionRevocationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CloseUserSessionsCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutAllCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var isUserValid = await _userRepository.IsUserValidAsync(request.UserId, asTracking, cancellationToken);
            if (!isUserValid)
            {
                _logger.LogWarning("GetUserSessionsQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var (userSessionsToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await _userSessionRevocationService.RevokeUserSessionsAsync(request.UserId,
                "Cierre de todas las sesiones de usuario ejecutado por el usuario administrador {currentUserId}.\"", currentUserId, utcNow, asTracking, cancellationToken);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userSessionsToRevoke.Count > 0)
                    await _unitOfWork.UserSessions.UpdateRangeAsync(userSessionsToRevoke, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
            }, cancellationToken: cancellationToken);
            return Result.Ok();
        }
    }
}