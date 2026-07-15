using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>
    {
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        private readonly IValidator<ChangePasswordCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IUserRepository _userRepository;
        private readonly IHashService _hashService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRepository _userSessionRepository;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, IValidator<ChangePasswordCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IHashService hashService, IDateTimeService dateTimeService,
            IUserSessionRepository userSessionRepository)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _hashService = hashService;
            _dateTimeService = dateTimeService;
            _userSessionRepository = userSessionRepository;
        }

        public async Task<Result<ChangePasswordResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("ChangePasswordCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<ChangePasswordResponse>.Fail(validation.ToCommandErrors());
            }
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var user = await _userRepository.GetByIdAsync(currentUserId, asTracking, cancellationToken);
            var isCurrentPasswordValid = _hashService.VerifyHash(request.CurrentPassword, user!.Salt, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("ChangePasswordCommand: contraseña actual incorrecta. UserId={UserId}", currentUserId);
                return Result<ChangePasswordResponse>.Fail(Error.Unauthorized("La contraseña actual es incorrecta."));
            }
            var isNewPasswordSameAsCurrent = _hashService.VerifyHash(request.NewPassword, user.Salt, user.PasswordHash);
            if (isNewPasswordSameAsCurrent)
            {
                _logger.LogWarning("ChangePasswordCommand: la nueva contraseña coincide con la actual. UserId={UserId}", currentUserId);
                return Result<ChangePasswordResponse>.Fail(Error.BusinessRule("La nueva contraseña debe ser diferente a la contraseña actual."));
            }
            var newSalt = _hashService.GenerateSalt();
            var newPasswordHash = _hashService.ComputeHash(request.NewPassword, newSalt);
            var utcNow = _dateTimeService.UtcNow;
            user.ChangePassword(newSalt, newPasswordHash, currentUserId, utcNow);
            var newSecurityStamp = Guid.NewGuid().ToString();
            user.UpdateSecurityCredentials(newSecurityStamp, user.TokenVersion + 1, currentUserId, utcNow);
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var activeUserSessions = (await _userSessionRepository.GetActiveByUserIdAsync(currentUserId, utcNow, asTracking, cancellationToken))
                .Where(us => us.Id != currentUserSessionId).ToList();





            throw new NotImplementedException();
        }
    }
}