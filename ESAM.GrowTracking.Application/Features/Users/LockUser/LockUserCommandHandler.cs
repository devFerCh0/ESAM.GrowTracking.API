using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.LockUser
{
    public class LockUserCommandHandler : IRequestHandler<LockUserCommand, Result>
    {
        private readonly ILogger<LockUserCommandHandler> _logger;
        private readonly IValidator<LockUserCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserService _userService;
        private readonly IUserSessionRevocationService _userSessionRevocationService;
        private readonly IUnitOfWork _unitOfWork;

        public LockUserCommandHandler(ILogger<LockUserCommandHandler> logger, IValidator<LockUserCommand> validator, 
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IDateTimeService dateTimeService, IUserService userService,
            IUserSessionRevocationService userSessionRevocationService, IUnitOfWork unitOfWork)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(userSessionRevocationService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _dateTimeService = dateTimeService;
            _userService = userService;
            _userSessionRevocationService = userSessionRevocationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(LockUserCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LockUserCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("LockUserCommand: intento de auto-bloqueo denegado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.BusinessRule("No es posible bloquear la propia cuenta."));
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("LockUserCommand: usuario objetivo no encontrado o eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("El usuario a bloquear no existe o se encuentra eliminado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            if (user.IsLocked(utcNow))
            {
                _logger.LogWarning("LockUserCommand: el usuario objetivo ya se encuentra bloqueado. UserId={UserId}, BloqueadoHasta={LockoutEndAt}", request.UserId, 
                    user.LockoutEndAt);
                return Result.Fail(Error.BusinessRule("El usuario ya se encuentra bloqueado."));
            }
            _userService.UserLock(user, request.LockoutEndAt, currentUserId, utcNow);
            var (userSessionsToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await _userSessionRevocationService.RevokeUserSessionsAsync(request.UserId, 
                "LockUser: Sesión finalizada por bloqueo administrativo de la cuenta.", currentUserId, utcNow, asTracking, cancellationToken);
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _unitOfWork.Users.UpdateAsync(user, ct);
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