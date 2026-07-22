using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Users.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly ILogger<DeleteUserCommandHandler> _logger;
        private readonly IValidator<DeleteUserCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserService _userService;
        private readonly IUserSessionRevocationService _userSessionRevocationService;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(ILogger<DeleteUserCommandHandler> logger, IValidator<DeleteUserCommand> validator,
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

        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("DeleteUserCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            if (request.UserId == currentUserId)
            {
                _logger.LogWarning("DeleteUserCommand: intento de eliminación de la propia cuenta denegado. UserId={UserId}", currentUserId);
                return Result.Fail(Error.BusinessRule("No es posible eliminar la propia cuenta."));
            }
            var asTracking = false;
            var user = await _userRepository.GetByIdAsync(request.UserId, asTracking, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("DeleteUserCommand: usuario objetivo no encontrado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.NotFound("El usuario a eliminar no existe."));
            }
            if (user.IsDeleted)
            {
                _logger.LogWarning("DeleteUserCommand: el usuario objetivo ya se encuentra eliminado. UserId={UserId}", request.UserId);
                return Result.Fail(Error.BusinessRule("El usuario ya se encuentra eliminado."));
            }
            var utcNow = _dateTimeService.UtcNow;
            _userService.UserDelete(user, currentUserId, utcNow);
            var (userSessionsToRevoke, userSessionRefreshTokensToRevoke, blacklistedRefreshTokens) = await _userSessionRevocationService.RevokeUserSessionsAsync(request.UserId,
                "DeleteUser: Sesión finalizada por eliminación administrativa de la cuenta.", currentUserId, utcNow, asTracking, cancellationToken);
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