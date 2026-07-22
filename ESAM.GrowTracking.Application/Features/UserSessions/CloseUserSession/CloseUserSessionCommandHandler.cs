using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSession
{
    public class CloseUserSessionCommandHandler : IRequestHandler<CloseUserSessionCommand, Result>
    {
        private readonly ILogger<CloseUserSessionCommandHandler> _logger;
        private readonly IValidator<CloseUserSessionCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRevocationService _userSessionRevocationService;
        private readonly IUnitOfWork _unitOfWork;

        public CloseUserSessionCommandHandler(ILogger<CloseUserSessionCommandHandler> logger, IValidator<CloseUserSessionCommand> validator, IUserRepository userRepository, 
            IUserSessionRepository userSessionRepository, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IDateTimeService dateTimeService,
            IUserSessionRevocationService userSessionRevocationService, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRevocationService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _userRepository = userRepository;
            _userSessionRepository = userSessionRepository;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _userSessionRevocationService = userSessionRevocationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CloseUserSessionCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("CloseUserSessionCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var asTracking = false;
            var isUserValid = await _userRepository.IsUserValidAsync(request.UserId, asTracking, cancellationToken);
            if (!isUserValid)
            {
                _logger.LogWarning("GetUserSessionsQuery: usuario no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("No se encontró el usuario especificado."));
            }
            var userSession = await _userSessionRepository.GetByIdAndUserIdAsync(request.UserSessionId, request.UserId, asTracking, cancellationToken);
            if (userSession is null)
            {
                _logger.LogWarning("CloseUserSessionCommand: sesión de usuario no encontrada o no pertenece al usuario especificado. UserSessionId={UserSessionId}, " +
                    "UserId={UserId}", request.UserSessionId, request.UserId);
                return Result.Fail(Error.NotFound("La sesión de usuario especificada no existe o no pertenece al usuario indicado."));
            }
            if (userSession.IsRevoked)
            {
                _logger.LogInformation("CloseUserSessionCommand: la sesión de usuario ya se encontraba revocada. UserSessionId={UserSessionId}, UserId={UserId}",
                    request.UserSessionId, request.UserId);
                return Result.Fail(Error.BusinessRule("La sesión de usuario indicada ya se encuentra revocada."));
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var utcNow = _dateTimeService.UtcNow;
            var (userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await _userSessionRevocationService.RevokeUserSessionAsync(userSession,
                $"Cierre de sesión de usuario ejecutada por el usuario administrador {currentUserId}.", currentUserId, utcNow, asTracking, cancellationToken);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.UserSessions.UpdateAsync(userSession, ct);
                if (userSessionRefreshTokensToRevoke.Count > 0)
                    await _unitOfWork.UserSessionRefreshTokens.UpdateRangeAsync(userSessionRefreshTokensToRevoke, ct);
                if (blacklistedRefreshTokens.Count > 0)
                    await _unitOfWork.BlacklistedRefreshTokens.InsertRangeAsync(blacklistedRefreshTokens, ct);
            }, cancellationToken: cancellationToken);
            return Result.Ok();
        }
    }
}