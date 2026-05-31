using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.Utilities;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IValidator<LoginCommand> _validator;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserRepository _userRepository;
        private readonly IUserDeviceRepository _userDeviceRepository;
        private readonly IClientInfoService _clientInfoService;
        private readonly AuthSecuritySettings _authSecuritySettings;
        private readonly IHashService _hashService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly IUserQuery _userQuery;

        public LoginCommandHandler(ILogger<LoginCommandHandler> logger, IValidator<LoginCommand> validator, IDateTimeService dateTimeService, IUserRepository userRepository,
            IUserDeviceRepository userDeviceRepository, IClientInfoService clientInfoService, IOptions<AuthSecuritySettings> authSecuritySettingsOptions,
            IHashService hashService, IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, IUserQuery userQuery)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userDeviceRepository);
            ArgumentNullException.ThrowIfNull(clientInfoService);
            ArgumentNullException.ThrowIfNull(authSecuritySettingsOptions);
            ArgumentNullException.ThrowIfNull(hashService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            ArgumentNullException.ThrowIfNull(userQuery);
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
            _clientInfoService = clientInfoService;
            _authSecuritySettings = authSecuritySettingsOptions.Value ?? throw new ArgumentNullException(nameof(authSecuritySettingsOptions));
            _hashService = hashService;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            _userQuery = userQuery;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LoginCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<LoginResponse>.Fail(validation.ToDomainErrors());
            }
            _ = EnumHelper.TryParseFromString<ApiClientType>(request.ApiClientType, out var apiClientType);
            var utcNow = _dateTimeService.UtcNow;
            var user = await _userRepository.GetByCredentialAsync(request.Credential!, asTracking, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("LoginCommand: autenticación fallida, usuario no encontrado para la credencial proporcionada.");
                return Result<LoginResponse>.Fail(Error.Unauthorized("Credenciales inválidas."));
            }
            if (user.IsDeleted)
            {
                _logger.LogWarning("LoginCommand: intento de inicio de sesión para usuario eliminado. UserId={UserId}", user.Id);
                return Result<LoginResponse>.Fail(Error.Forbidden("La cuenta está desactivada. Contacte con el administrador."));
            }
            if (user.IsLocked(utcNow))
            {
                _logger.LogWarning("LoginCommand: intento de inicio de sesión para usuario bloqueado. UserId={UserId}, BloqueadoHasta={LockoutEndAt}", user.Id, user.LockoutEndAt);
                return Result<LoginResponse>.Fail(Error.Locked($"Cuenta bloqueada hasta {user.LockoutEndAt}."));
            }
            var userDevice = await _userDeviceRepository.GetByUserIdAndDeviceIdentifierAsync(user.Id, request.DeviceIdentifier!, asTracking, cancellationToken);
            var ipAddress = _clientInfoService.GetIpAddress();
            var userAgent = _clientInfoService.GetUserAgent();
            if (userDevice is null)
                userDevice = new UserDevice(user.Id, request.DeviceIdentifier!, request.DeviceName!, apiClientType, ipAddress, userAgent, user.Id, utcNow);
            else
            {
                userDevice.Update(request.DeviceName!, apiClientType, ipAddress, userAgent, user.Id, utcNow);
                if (userDevice.IsDeleted)
                    userDevice.Activate(user.Id, utcNow);
            }
            userDevice.UpdateLastSeenAt(utcNow, user.Id, utcNow);
            var wasFailedLoginReset = false;
            if (userDevice.ShouldResetFailedAttempts(_authSecuritySettings.FailedAttemptsResetDuration, utcNow) && userDevice.FailedLoginCount > 0)
            {
                userDevice.ResetFailedLogin(user.Id, utcNow);
                wasFailedLoginReset = true;
            }
            if (userDevice.IsLocked(utcNow))
            {
                await PersistDeviceAsync(userDevice, cancellationToken);
                _logger.LogWarning("LoginCommand: intento de inicio de sesión desde dispositivo bloqueado. UserId={UserId}, UserDeviceId={UserDeviceId}, " +
                    "BloqueadoHasta={LockoutEndAt}", user.Id, userDevice.Id, userDevice.LockoutEndAt);
                return Result<LoginResponse>.Fail(Error.Locked($"Dispositivo bloqueado hasta {userDevice.LockoutEndAt}."));
            }
            var isPasswordValid = _hashService.VerifyHash(request.Password!, user.Salt, user.PasswordHash);
            if (!isPasswordValid)
            {
                userDevice.RegisterFailedLogin(_authSecuritySettings.MaxFailedAttempts, _authSecuritySettings.LockoutDuration, utcNow, user.Id, utcNow);
                await PersistDeviceAsync(userDevice, cancellationToken);
                var remainingAttempts = Math.Max(0, _authSecuritySettings.MaxFailedAttempts - userDevice.FailedLoginCount);
                if (remainingAttempts > 0)
                {
                    _logger.LogWarning("LoginCommand: contraseña incorrecta. UserId={UserId}, UserDeviceId={UserDeviceId}, IntentosFallidos={FailedLoginCount}, " +
                        "IntentosRestantes={RemainingAttempts}", user.Id, userDevice.Id, userDevice.FailedLoginCount, remainingAttempts);
                    return Result<LoginResponse>.Fail(Error.Unauthorized($"Contraseña incorrecta. Le quedan {remainingAttempts} intento(s)."));
                }
                _logger.LogWarning("LoginCommand: dispositivo bloqueado por demasiados intentos fallidos. UserId={UserId}, UserDeviceId={UserDeviceId}, " +
                    "BloqueadoHasta={LockoutEndAt}", user.Id, userDevice.Id, userDevice.LockoutEndAt);
                return Result<LoginResponse>.Fail(Error.Locked("Se ha superado el número de intentos permitidos. Dispositivo bloqueado temporalmente."));
            }
            if (!wasFailedLoginReset && userDevice.FailedLoginCount > 0)
                userDevice.ResetFailedLogin(user.Id, utcNow);
            userDevice.UpdateLastLogin(utcNow, user.Id, utcNow);
            await PersistDeviceAsync(userDevice, cancellationToken);
            var accessToken = _tokenService.GenerateTemporaryAccessToken(user.Id, user.SecurityStamp, user.TokenVersion, userDevice.Id, request.IsPersistent!.Value, utcNow,
                _tokenLifetimeSettings.TemporaryAccessTokenLifetimeMinutes);
            var loginUser = await _userQuery.GetLoginUserByIdAsync(user.Id, asTracking, cancellationToken);
            if (loginUser is null)
            {
                _logger.LogError("LoginCommand: datos del usuario no encontrados tras autenticación exitosa. UserId={UserId}", user.Id);
                return Result<LoginResponse>.Fail(Error.ServerError("Usuario no encontrado."));
            }
            if (loginUser.UserWorkProfiles is null || loginUser.UserWorkProfiles.Count == 0)
            {
                _logger.LogWarning("LoginCommand: el usuario no tiene perfiles de trabajo asignados. UserId={UserId}", user.Id);
                return Result<LoginResponse>.Fail(Error.BusinessRule("Usuario sin perfiles de trabajo asignados."));
            }
            return Result<LoginResponse>.Ok(new LoginResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, loginUser));
        }

        private async Task PersistDeviceAsync(UserDevice userDevice, CancellationToken cancellationToken)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (userDevice.Id == 0)
                    await _unitOfWork.UserDevices.InsertAsync(userDevice, ct);
                else
                    await _unitOfWork.UserDevices.UpdateAsync(userDevice, ct);
            }, cancellationToken: cancellationToken);
        }
    }
}