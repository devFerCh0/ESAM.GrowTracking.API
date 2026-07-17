using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, IValidator<ChangePasswordCommand> validator,
            IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService, IUserRepository userRepository, IHashService hashService, IDateTimeService dateTimeService,
            IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, ITokenService tokenService, 
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _userRepository = userRepository;
            _hashService = hashService;
            _dateTimeService = dateTimeService;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
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
            var utcNow = _dateTimeService.UtcNow;
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var activeUserSessions = (await _userSessionRepository.GetActiveByUserIdAsync(currentUserId, utcNow, asTracking, cancellationToken))
                .Where(us => us.Id != currentUserSessionId).ToList();
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var (securityStamp, tokenVersion) = await _userSessionService.ChangePassworsAndRevokeCurrentUserSessionsAndAccessTokenSessionAsync(activeUserSessions, user, 
                request.NewPassword, "Cambio de contraseña: Access token anterior revocado por rotación de credenciales de seguridad.", currentJti, currentAccessTokenExpiration, 
                currentUserId, currentUserSessionId, utcNow, asTracking, cancellationToken);
            var currentWorkProfileType = _accessTokenClaimsValidatorService.CurrentWorkProfileType;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            var currentWorkProfileSelectedId = _accessTokenClaimsValidatorService.CurrentWorkProfileSelectedId;
            var currentWorkProfileId = _accessTokenClaimsValidatorService.CurrentWorkProfileId;
            var accessToken = currentWorkProfileType == WorkProfileType.OnlyWorkProfile
                ? _tokenService.GenerateSessionAccessToken(currentUserId, securityStamp, tokenVersion, currentUserDeviceId, currentUserSessionId, utcNow,
                    _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentWorkProfileSelectedId, currentWorkProfileId, currentWorkProfileType)
                : _tokenService.GenerateSessionAccessToken(currentUserId, securityStamp, tokenVersion, currentUserDeviceId, currentUserSessionId, utcNow,
                    _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, currentWorkProfileSelectedId, currentWorkProfileId, currentWorkProfileType,
                    _accessTokenClaimsValidatorService.CurrentRoleCampusSelectedId, _accessTokenClaimsValidatorService.CurrentRoleId, 
                    _accessTokenClaimsValidatorService.CurrentCampusId);
            _logger.LogInformation("ChangePasswordCommand: contraseña actualizada exitosamente. UserId={UserId}", currentUserId);
            return Result<ChangePasswordResponse>.Ok(new ChangePasswordResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt));
        }
    }
}