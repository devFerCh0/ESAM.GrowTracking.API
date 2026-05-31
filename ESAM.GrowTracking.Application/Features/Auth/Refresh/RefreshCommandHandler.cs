using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Application.Settings;
using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResponse>>
    {
        private readonly ILogger<RefreshCommandHandler> _logger;
        private readonly IValidator<RefreshCommand> _validator;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlacklistedTokenService _blacklistedTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly ITokenService _tokenService;
        private readonly TokenLifetimeSettings _tokenLifetimeSettings;
        private readonly ITokenSessionValidationService _tokenSessionValidationService;

        public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, IDateTimeService dateTimeService,
            IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService,
            IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, ITokenService tokenService,
            IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, ITokenSessionValidationService tokenSessionValidationService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(blacklistedTokenService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(tokenService);
            ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
            ArgumentNullException.ThrowIfNull(tokenSessionValidationService);
            _logger = logger;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _currentUserService = currentUserService;
            _blacklistedTokenService = blacklistedTokenService;
            _unitOfWork = unitOfWork;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _tokenService = tokenService;
            _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
            _tokenSessionValidationService = tokenSessionValidationService;
        }

        public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var asTracking = false;
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("RefreshCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result<RefreshResponse>.Fail(validation.ToDomainErrors());
            }
            var utcNow = _dateTimeService.UtcNow;
            RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var refreshTokenPlain);
            var userSessionRefreshToken = identifier is not null ? await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken) : null;
            int currentUserId;
            string? currentJti = null;
            DateTime? currentAccessTokenExpiration = null;
            UserSession? userSession;
            string revokedReasonPrefix;
            if (_currentUserService.IsAuthenticated)
            {
                currentUserId = _currentUserService.UserId!.Value;
                var currentUserSessionId = _currentUserService.UserSessionId!.Value;
                var currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
                currentJti = _currentUserService.Jti;
                currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
                revokedReasonPrefix = "Refresh (Autenticado):";
                if (userSessionRefreshToken is null)
                {
                    var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
                    if (currentAccessTokenType == AccessTokenType.Temporary)
                    {
                        var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
                            currentAccessTokenExpiration!.Value, utcNow, revokedReasonPrefix + " Access token temporal revocado.", currentUserId, utcNow, asTracking,
                            cancellationToken);
                        if (blacklistedAccessTokenTemporary is not null)
                        {
                            await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("No autorizado para refresh con el token actual."));
                    }
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                        cancellationToken);
                    if (targetUserSession is not null)
                    {
                        _logger.LogWarning("RefreshCommand: refresh token no encontrado para sesión autenticada, revocando sesión. UserId={UserId}, UserSessionId={UserSessionId}",
                            currentUserId, currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
                            revokedReasonPrefix + " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.",
                            currentUserId, utcNow, asTracking, cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, la sesión actual ha sido revocada por seguridad."));
                    }
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión activa para realizar refresh."));
                }
                userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
                {
                    _logger.LogWarning("RefreshCommand: discrepancia entre sesión y refresh token en solicitud autenticada. UserId={UserId}, " +
                        "IdentificadorProvisto={Identifier}, UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!,
                        userSessionRefreshToken.UserSessionId, currentUserSessionId);
                    var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
                        cancellationToken);
                    if (targetUserSession is not null)
                    {
                        _logger.LogWarning("RefreshCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId,
                            currentUserSessionId);
                        await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration,
                            revokedReasonPrefix + " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.", currentUserId, utcNow, asTracking,
                            cancellationToken);
                        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, se ha revocado la sesión actual por seguridad."));
                    }
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión válida para el token proporcionado."));
                }
            }
            else
            {
                if (userSessionRefreshToken is null)
                {
                    _logger.LogWarning("RefreshCommand (No Autenticado): no se proporcionó identificador de token.");
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Token de refresh no provisto o inválido."));
                }
                userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                if (userSession is null)
                {
                    _logger.LogWarning("RefreshCommand (No Autenticado): identificador de token provisto pero sesión no encontrada. Identificador={Identifier}", identifier!);
                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Token inválido o sesión asociada no encontrada."));
                }
                currentUserId = userSession.UserId;
                revokedReasonPrefix = "Refresh (No Autenticado):";
            }
            var tokenSessionValidationResult = await _tokenSessionValidationService.ValidateAsync(userSession, userSessionRefreshToken!, refreshTokenPlain, 
                request.DeviceIdentifier, revokedReasonPrefix, _currentUserService.IsAuthenticated, 
                currentUserId, _currentUserService.IsAuthenticated ? _currentUserService.WorkProfileId : null, 
                _currentUserService.IsAuthenticated ? _currentUserService.RoleId : null, _currentUserService.IsAuthenticated ? _currentUserService.CampusId : null,
                _currentUserService.IsAuthenticated ? _currentUserService.TokenVersion : null, _currentUserService.IsAuthenticated ? _currentUserService.SecurityStamp : null,
                utcNow, asTracking, cancellationToken);
            if (!tokenSessionValidationResult.IsValid)
            {
                await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.RevokedReason, currentUserId, 
                    utcNow, asTracking, cancellationToken);
                return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido. La sesión ha sido revocada por motivos de seguridad. Inicie sesión nuevamente."));
            }
            return await ExecuteTokenRotationAsync(userSession, userSessionRefreshToken!, tokenSessionValidationResult.ValidatedUser!, tokenSessionValidationResult.RevokedReason, 
                currentUserId, utcNow, currentJti, currentAccessTokenExpiration, tokenSessionValidationResult.WorkProfileId, tokenSessionValidationResult.RoleId, 
                tokenSessionValidationResult.CampusId, asTracking, cancellationToken);
        }

        private async Task<Result<RefreshResponse>> ExecuteTokenRotationAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, User user, 
            string revokedReason, int currentUserId, DateTime utcNow, string? currentJti, DateTime? currentAccessTokenExpiration, int workProfileId, int? roleId = null, 
            int? campusId = null, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var rotatedRefreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti, currentAccessTokenExpiration, 
                revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
            var accessToken = _tokenService.GenerateSessionAccessToken(user.Id, user.SecurityStamp, user.TokenVersion, userSession.UserDeviceId, userSession.Id, utcNow,
                _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileId, roleId, campusId);
            return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, rotatedRefreshToken.Identifier,
                rotatedRefreshToken.Token, rotatedRefreshToken.ExpiresIn, rotatedRefreshToken.ExpiresAt));
        }
    }

    //public class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResponse>>
    //{
    //    private readonly ILogger<RefreshCommandHandler> _logger;
    //    private readonly IValidator<RefreshCommand> _validator;
    //    private readonly IDateTimeService _dateTimeService;
    //    private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
    //    private readonly ICurrentUserService _currentUserService;
    //    private readonly IBlacklistedTokenService _blacklistedTokenService;
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly IUserSessionRepository _userSessionRepository;
    //    private readonly IUserSessionService _userSessionService;
    //    private readonly IHashService _hashService;
    //    private readonly IUserRepository _userRepository;
    //    private readonly IUserDeviceRepository _userDeviceRepository;
    //    private readonly IUserSessionWorkProfileSelectedRepository _userSessionWorkProfileSelectedRepository;
    //    private readonly IUserWorkProfileRepository _userWorkProfileRepository;
    //    private readonly IWorkProfileRepository _workProfileRepository;
    //    private readonly IUserSessionRoleCampusSelectedRepository _userSessionRoleCampusSelectedRepository;
    //    private readonly IWorkProfilePermissionRepository _workProfilePermissionRepository;
    //    private readonly ITokenService _tokenService;
    //    private readonly TokenLifetimeSettings _tokenLifetimeSettings;
    //    private readonly IUserRoleCampusRepository _userRoleCampusRepository;
    //    private readonly IRolePermissionRepository _rolePermissionRepository;

    //    public RefreshCommandHandler(ILogger<RefreshCommandHandler> logger, IValidator<RefreshCommand> validator, IDateTimeService dateTimeService, 
    //        IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository, ICurrentUserService currentUserService, IBlacklistedTokenService blacklistedTokenService, 
    //        IUnitOfWork unitOfWork, IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, IHashService hashService, IUserRepository userRepository, 
    //        IUserDeviceRepository userDeviceRepository, IUserSessionWorkProfileSelectedRepository userSessionWorkProfileSelectedRepository, 
    //        IUserWorkProfileRepository userWorkProfileRepository, IWorkProfileRepository workProfileRepository,
    //        IUserSessionRoleCampusSelectedRepository userSessionRoleCampusSelectedRepository, 
    //        IWorkProfilePermissionRepository workProfilePermissionRepository, ITokenService tokenService, IOptions<TokenLifetimeSettings> tokenLifetimeSettingsOptions, 
    //        IUserRoleCampusRepository userRoleCampusRepository, IRolePermissionRepository rolePermissionRepository)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(validator);
    //        ArgumentNullException.ThrowIfNull(dateTimeService);
    //        ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
    //        ArgumentNullException.ThrowIfNull(currentUserService);
    //        ArgumentNullException.ThrowIfNull(blacklistedTokenService);
    //        ArgumentNullException.ThrowIfNull(unitOfWork);
    //        ArgumentNullException.ThrowIfNull(userSessionRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionService);
    //        ArgumentNullException.ThrowIfNull(hashService);
    //        ArgumentNullException.ThrowIfNull(userRepository);
    //        ArgumentNullException.ThrowIfNull(userDeviceRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionWorkProfileSelectedRepository);
    //        ArgumentNullException.ThrowIfNull(userWorkProfileRepository);
    //        ArgumentNullException.ThrowIfNull(workProfileRepository);
    //        ArgumentNullException.ThrowIfNull(userSessionRoleCampusSelectedRepository);
    //        ArgumentNullException.ThrowIfNull(workProfilePermissionRepository);
    //        ArgumentNullException.ThrowIfNull(tokenService);
    //        ArgumentNullException.ThrowIfNull(tokenLifetimeSettingsOptions);
    //        ArgumentNullException.ThrowIfNull(userRoleCampusRepository);
    //        ArgumentNullException.ThrowIfNull(rolePermissionRepository);
    //        _logger = logger;
    //        _validator = validator;
    //        _dateTimeService = dateTimeService;
    //        _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
    //        _currentUserService = currentUserService;
    //        _blacklistedTokenService = blacklistedTokenService;
    //        _unitOfWork = unitOfWork;
    //        _userSessionRepository = userSessionRepository;
    //        _userSessionService = userSessionService;
    //        _hashService = hashService;
    //        _userRepository = userRepository;
    //        _userDeviceRepository = userDeviceRepository;
    //        _userSessionWorkProfileSelectedRepository = userSessionWorkProfileSelectedRepository;
    //        _userWorkProfileRepository = userWorkProfileRepository;
    //        _workProfileRepository = workProfileRepository;
    //        _userSessionRoleCampusSelectedRepository = userSessionRoleCampusSelectedRepository;
    //        _workProfilePermissionRepository = workProfilePermissionRepository;
    //        _tokenService = tokenService;
    //        _tokenLifetimeSettings = tokenLifetimeSettingsOptions.Value ?? throw new ArgumentNullException(nameof(tokenLifetimeSettingsOptions));
    //        _userRoleCampusRepository = userRoleCampusRepository;
    //        _rolePermissionRepository = rolePermissionRepository;
    //    }

    //    public async Task<Result<RefreshResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    //    {
    //        var asTracking = false;
    //        var validation = await _validator.ValidateAsync(request, cancellationToken);
    //        if (!validation.IsValid)
    //        {
    //            _logger.LogWarning("RefreshCommand: validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
    //            return Result<RefreshResponse>.Fail(validation.ToDomainErrors());
    //        }
    //        var utcNow = _dateTimeService.UtcNow;
    //        int currentUserId;
    //        string? currentJti = null;
    //        DateTime? currentAccessTokenExpiration = null;
    //        string revokedReason;
    //        UserSession? userSession = null;
    //        UserSessionRefreshToken? userSessionRefreshToken = null;
    //        string? identifier = null;
    //        string? refreshTokenPlain = null;
    //        if (!string.IsNullOrWhiteSpace(request.RefreshTokenRaw))
    //        {
    //            var refreshTokenRaws = request.RefreshTokenRaw.Split('.');
    //            if (refreshTokenRaws.Length == 2)
    //            {
    //                identifier = refreshTokenRaws[0];
    //                refreshTokenPlain = refreshTokenRaws[1];
    //                userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
    //            }
    //        }
    //        if (_currentUserService.IsAuthenticated)
    //        {
    //            currentUserId = _currentUserService.UserId!.Value;
    //            int currentUserSessionId = _currentUserService.UserSessionId!.Value;
    //            int currentUserDeviceId = _currentUserService.UserDeviceId!.Value;
    //            currentJti = _currentUserService.Jti;
    //            currentAccessTokenExpiration = _currentUserService.AccessTokenExpiration;
    //            revokedReason = "Refresh (Autenticado):";
    //            if (userSessionRefreshToken is null)
    //            {
    //                var currentAccessTokenType = _currentUserService.AccessTokenType!.Value;
    //                if (currentAccessTokenType == AccessTokenType.Temporary)
    //                {
    //                    revokedReason += " Access token temporal revocado.";
    //                    var blacklistedAccessTokenTemporary = await _blacklistedTokenService.TryGenerateBlacklistedAccessTokenTemporaryAsync(currentUserId, currentJti!,
    //                        currentAccessTokenExpiration!.Value, utcNow, revokedReason, currentUserId, utcNow, asTracking, cancellationToken);
    //                    if (blacklistedAccessTokenTemporary is not null)
    //                    {
    //                        await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
    //                        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //                    }
    //                    return Result<RefreshResponse>.Fail(Error.Unauthorized("No autorizado para refresh con el token actual."));
    //                }
    //                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking, 
    //                    cancellationToken);
    //                if (targetUserSession is not null)
    //                {
    //                    revokedReason += " Refresh token no encontrado para esta sesión; se revoca la sesión actual por seguridad.";
    //                    _logger.LogWarning("RefreshCommand: refresh token no encontrado para sesión autenticada, revocando sesión. UserId={UserId}, UserSessionId={UserSessionId}", 
    //                        currentUserId, currentUserSessionId);
    //                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration, revokedReason, currentUserId, utcNow, 
    //                        asTracking, cancellationToken);
    //                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, la sesión actual ha sido revocada por seguridad."));
    //                }
    //                return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión activa para realizar refresh."));
    //            }
    //            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //            if (userSession is null || userSession.Id != currentUserSessionId || userSession.UserId != currentUserId || userSession.UserDeviceId != currentUserDeviceId)
    //            {
    //                _logger.LogWarning("RefreshCommand: discrepancia entre sesión y refresh token en solicitud autenticada. UserId={UserId}, IdentificadorProvisto={Identifier}, " + 
    //                    "UserSessionIdDelToken={TokenUserSessionId}, UserSessionIdActual={CurrentUserSessionId}", currentUserId, identifier!, userSessionRefreshToken.UserSessionId, 
    //                    currentUserSessionId);
    //                var targetUserSession = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking, 
    //                    cancellationToken);
    //                if (targetUserSession is not null)
    //                {
    //                    _logger.LogWarning("RefreshCommand: revocando sesión actual por discrepancia. UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, 
    //                        currentUserSessionId);
    //                    revokedReason += " Refresh token o sesión no coincidentes; se revoca la sesión actual por seguridad.";
    //                    await _userSessionService.RevokeUserSessionAsync(targetUserSession, currentJti, currentAccessTokenExpiration, revokedReason, currentUserId, utcNow, 
    //                        asTracking, cancellationToken);
    //                    return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido, se ha revocado la sesión actual por seguridad."));
    //                }
    //                return Result<RefreshResponse>.Fail(Error.Unauthorized("No se encontró una sesión válida para el token proporcionado."));
    //            }
    //        }
    //        else
    //        {
    //            if (userSessionRefreshToken is null)
    //            {
    //                _logger.LogWarning("RefreshCommand (No Autenticado): no se proporcionó identificador de token.");
    //                return Result<RefreshResponse>.Fail(Error.Unauthorized("Token de refresh no provisto o inválido."));
    //            }
    //            userSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
    //            if (userSession is null)
    //            {
    //                _logger.LogWarning("RefreshCommand (No Autenticado): identificador de token provisto pero sesión no encontrada. Identificador={Identifier}", identifier!);
    //                return Result<RefreshResponse>.Fail(Error.Unauthorized("Token inválido o sesión asociada no encontrada."));
    //            }
    //            currentUserId = userSession.UserId;
    //            revokedReason = "Refresh (No Autenticado):";
    //        }
    //        if (userSession.AbsoluteExpiresAt <= utcNow)
    //            revokedReason += " Sesión absoluta expirada.";
    //        else if (userSessionRefreshToken.ExpiresAt <= utcNow || userSession.ExpiresAt <= utcNow)
    //            revokedReason += " Refresh token o sesión expirada.";
    //        else if (userSessionRefreshToken.IsRevoked || userSession.IsRevoked)
    //            revokedReason += " Refresh token o sesión ya revocada.";
    //        else if (!_hashService.VerifyHash(refreshTokenPlain!, userSessionRefreshToken.Salt, userSessionRefreshToken.TokenHash))
    //        {
    //            revokedReason += " Refresh token inválido.";
    //            _logger.LogWarning("RefreshCommand: hash del refresh token no coincide. UserSessionId={UserSessionId}, Identificador={Identifier}", userSession.Id, identifier!);
    //        }
    //        else if (userSessionRefreshToken.ReplacedByUserSessionRefreshTokenId.HasValue)
    //            revokedReason += " Refresh token reemplazado.";
    //        else
    //        {
    //            var user = await _userRepository.GetByIdAsync(userSession.UserId, asTracking, cancellationToken);
    //            if (user is null || user.IsDeleted || user.IsLocked(utcNow))
    //            {
    //                revokedReason += " Usuario no encontrado, deshabilitado o bloqueado.";
    //                _logger.LogWarning("RefreshCommand: usuario inválido para refresh. UserSessionId={UserSessionId}, UserId={UserId}", userSession.Id, userSession.UserId);
    //            }
    //            else
    //            {
    //                var userDevice = await _userDeviceRepository.GetByIdAndUserIdAsync(userSession.UserDeviceId, userSession.UserId, asTracking, cancellationToken);
    //                if (userDevice is null || userDevice.IsDeleted || userDevice.IsLocked(utcNow))
    //                {
    //                    revokedReason += " Dispositivo no encontrado, deshabilitado o bloqueado.";
    //                    _logger.LogWarning("RefreshCommand: dispositivo inválido para refresh. UserSessionId={UserSessionId}, UserDeviceId={UserDeviceId}", 
    //                        userSession.Id, userSession.UserDeviceId);
    //                }
    //                else if (!string.Equals(request.DeviceIdentifier, userDevice.DeviceIdentifier, StringComparison.Ordinal))
    //                {
    //                    revokedReason += " DeviceIdentifier no coincide.";
    //                    _logger.LogWarning("RefreshCommand: identificador de dispositivo no coincide. Esperado={Expected}, Provisto={Provided}, UserSessionId={UserSessionId}", 
    //                        userDevice.DeviceIdentifier, request.DeviceIdentifier, userSession.Id);
    //                }
    //                else
    //                {
    //                    var userSessionWorkProfileSelected = await _userSessionWorkProfileSelectedRepository.GetByUserSessionIdAsync(userSession.Id, asTracking, cancellationToken);
    //                    if (userSessionWorkProfileSelected is null)
    //                    {
    //                        revokedReason += " La sesión no tiene perfil de trabajo seleccionado.";
    //                        _logger.LogWarning("RefreshCommand: la sesión no tiene perfil de trabajo seleccionado. UserSessionId={UserSessionId}", userSession.Id);
    //                    }
    //                    else
    //                    {
    //                        var userWorkProfile = await _userWorkProfileRepository.GetByUserIdAndWorkProfileIdAsync(userSessionWorkProfileSelected.UserId,
    //                            userSessionWorkProfileSelected.WorkProfileId, asTracking, cancellationToken);
    //                        if (userWorkProfile is null || userWorkProfile.IsDeleted)
    //                        {
    //                            revokedReason += " Perfil de trabajo no activo o no asignado.";
    //                            _logger.LogWarning("RefreshCommand: perfil de trabajo inválido para refresh. UserSessionId={UserSessionId}, WorkProfileId={WorkProfileId}", 
    //                                userSession.Id, userSessionWorkProfileSelected.WorkProfileId);
    //                        }
    //                        else
    //                        {
    //                            var workProfileType = await _workProfileRepository.GetWorkProfileTypeByIdAsync(userWorkProfile.WorkProfileId, asTracking, cancellationToken);
    //                            if (workProfileType != WorkProfileType.WithRoles && workProfileType != WorkProfileType.OnlyWorkProfile)
    //                            {
    //                                revokedReason += " Tipo de perfil de trabajo no válido.";
    //                                _logger.LogWarning("RefreshCommand: tipo de perfil de trabajo inválido. WorkProfileId={WorkProfileId}, Tipo={Type}", 
    //                                    userWorkProfile.WorkProfileId, workProfileType);
    //                            }
    //                            else
    //                            {
    //                                var userSessionRoleCampusSelected = await _userSessionRoleCampusSelectedRepository.GetByUserSessionIdAsync(userSession.Id, asTracking, 
    //                                    cancellationToken);
    //                                if (workProfileType == WorkProfileType.OnlyWorkProfile)
    //                                {
    //                                    if (userSessionRoleCampusSelected is not null)
    //                                    {
    //                                        revokedReason += " Tipo OnlyWorkProfile No debe tener rol de sede seleccionado.";
    //                                        _logger.LogWarning("RefreshCommand: tipo OnlyWorkProfile no debe tener rol de sede seleccionado. UserSessionId={UserSessionId}", 
    //                                            userSession.Id);
    //                                    }
    //                                    else
    //                                    {
    //                                        var hasActivePermissions = await _workProfilePermissionRepository.HasActivePermissionsAsync(userWorkProfile.WorkProfileId, asTracking, 
    //                                            cancellationToken);
    //                                        if (!hasActivePermissions)
    //                                        {
    //                                            revokedReason += " Perfil de trabajo no tiene permisos asignados.";
    //                                            _logger.LogWarning("RefreshCommand: perfil de trabajo sin permisos activos. WorkProfileId={WorkProfileId}", 
    //                                                userWorkProfile.WorkProfileId);
    //                                        }
    //                                        else
    //                                        {
    //                                            if (_currentUserService.IsAuthenticated)
    //                                            {
    //                                                int currentTokenVersion = _currentUserService.TokenVersion!.Value;
    //                                                string currentSecurityStamp = _currentUserService.SecurityStamp!;
    //                                                int currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
    //                                                if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
    //                                                {
    //                                                    revokedReason += " SecurityStamp o TokenVersion no coincidentes con el contexto actual.";
    //                                                    _logger.LogWarning("RefreshCommand: discrepancia de contexto de seguridad en refresh autenticado OnlyWorkProfile. " + 
    //                                                        "UserId={UserId}", user.Id);
    //                                                }
    //                                                else if (userSessionWorkProfileSelected.WorkProfileId != currentWorkProfileId)
    //                                                {
    //                                                    revokedReason += " Perfil seleccionado en la sesión no coincide con el contexto autenticado.";
    //                                                    _logger.LogWarning("RefreshCommand: perfil de trabajo seleccionado no coincide con el contexto autenticado " + 
    //                                                        "OnlyWorkProfile. UserSessionId={UserSessionId}", userSession.Id);
    //                                                }
    //                                                else
    //                                                    return await PerformRotationAsync(userSession, userSessionRefreshToken, user, revokedReason, currentUserId, utcNow,
    //                                                        currentJti, currentAccessTokenExpiration, userSessionWorkProfileSelected.WorkProfileId, asTracking: asTracking, 
    //                                                        cancellationToken: cancellationToken);
    //                                            }
    //                                            else
    //                                                return await PerformRotationAsync(userSession, userSessionRefreshToken, user, revokedReason, currentUserId, utcNow,
    //                                                    currentJti, currentAccessTokenExpiration, userSessionWorkProfileSelected.WorkProfileId, asTracking: asTracking, 
    //                                                    cancellationToken: cancellationToken);
    //                                        }
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    if (userSessionRoleCampusSelected is null)
    //                                    {
    //                                        revokedReason += " Tipo WithRoles requiere rol de sede seleccionado.";
    //                                        _logger.LogWarning("RefreshCommand: sesión de tipo WithRoles sin rol de sede seleccionado. UserSessionId={UserSessionId}", 
    //                                            userSession.Id);
    //                                    }
    //                                    else
    //                                    {
    //                                        var userRoleCampus = await _userRoleCampusRepository.GetByUserIdAndRoleIdAndCampusIdAsync(userSessionRoleCampusSelected.UserId, 
    //                                            userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId, asTracking, cancellationToken);
    //                                        if (userRoleCampus is null || userRoleCampus.IsDeleted)
    //                                        {
    //                                            revokedReason += " Rol de sede no activo o no asignado.";
    //                                            _logger.LogWarning("RefreshCommand: rol de sede inválido para refresh. UserSessionId={UserSessionId}, RoleId={RoleId}, " + 
    //                                                "CampusId={CampusId}", userSession.Id, userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId);
    //                                        }
    //                                        else
    //                                        {
    //                                            var hasActivePermissions = await _rolePermissionRepository.HasActivePermissionsAsync(userRoleCampus.RoleId, asTracking, 
    //                                                cancellationToken);
    //                                            if (!hasActivePermissions)
    //                                            {
    //                                                revokedReason += " Rol no tiene permisos asignados.";
    //                                                _logger.LogWarning("RefreshCommand: rol sin permisos activos. RoleId={RoleId}", userRoleCampus.RoleId);
    //                                            }
    //                                            else
    //                                            {
    //                                                if (_currentUserService.IsAuthenticated)
    //                                                {
    //                                                    int currentTokenVersion = _currentUserService.TokenVersion!.Value;
    //                                                    string currentSecurityStamp = _currentUserService.SecurityStamp!;
    //                                                    int currentWorkProfileId = _currentUserService.WorkProfileId!.Value;
    //                                                    int currentRoleId = _currentUserService.RoleId!.Value;
    //                                                    int currentCampusId = _currentUserService.CampusId!.Value;
    //                                                    if (user.SecurityStamp != currentSecurityStamp || user.TokenVersion != currentTokenVersion)
    //                                                    {
    //                                                        revokedReason += " SecurityStamp o TokenVersion no coincidentes con el contexto actual.";
    //                                                        _logger.LogWarning("RefreshCommand: discrepancia de contexto de seguridad en refresh autenticado WithRoles. " + 
    //                                                            "UserId={UserId}", user.Id);
    //                                                    }
    //                                                    else if (userSessionWorkProfileSelected.WorkProfileId != currentWorkProfileId)
    //                                                    {
    //                                                        revokedReason += " Perfil seleccionado en la sesión no coincide con el contexto autenticado.";
    //                                                        _logger.LogWarning("RefreshCommand: perfil de trabajo seleccionado no coincide con el contexto autenticado " + 
    //                                                            "WithRoles. UserSessionId={UserSessionId}", userSession.Id);
    //                                                    }
    //                                                    else if (userSessionRoleCampusSelected.RoleId != currentRoleId || userSessionRoleCampusSelected.CampusId != currentCampusId)
    //                                                    {
    //                                                        revokedReason += " Rol de sede seleccionado en la sesión no coincide con el contexto autenticado.";
    //                                                        _logger.LogWarning("RefreshCommand: rol de sede no coincide con el contexto autenticado. UserSessionId={UserSessionId}", 
    //                                                            userSession.Id);
    //                                                    }
    //                                                    else
    //                                                        return await PerformRotationAsync(userSession, userSessionRefreshToken, user, revokedReason, currentUserId, utcNow,
    //                                                            currentJti, currentAccessTokenExpiration, userSessionWorkProfileSelected.WorkProfileId, 
    //                                                            userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId, asTracking, cancellationToken);
    //                                                }
    //                                                else
    //                                                    return await PerformRotationAsync(userSession, userSessionRefreshToken, user, revokedReason, currentUserId, utcNow,
    //                                                        currentJti, currentAccessTokenExpiration, userSessionWorkProfileSelected.WorkProfileId, 
    //                                                        userSessionRoleCampusSelected.RoleId, userSessionRoleCampusSelected.CampusId, asTracking, cancellationToken);
    //                                            }
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        await _userSessionService.RevokeUserSessionAsync(userSession, currentJti, currentAccessTokenExpiration, revokedReason, currentUserId, utcNow, asTracking, 
    //            cancellationToken);
    //        return Result<RefreshResponse>.Fail(Error.Unauthorized("Refresh inválido. La sesión ha sido revocada por motivos de seguridad. Inicie sesión nuevamente."));
    //    }

    //    private async Task<Result<RefreshResponse>> PerformRotationAsync(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken, User user, string revokedReason, 
    //        int currentUserId, DateTime utcNow, string? currentJti, DateTime? currentAccessTokenExpiration, int workProfileId, int? roleId = null, int? campusId = null, 
    //        bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var refreshToken = await _userSessionService.RotateUserSessionAsync(userSession, userSessionRefreshToken, currentJti, currentAccessTokenExpiration, 
    //            revokedReason + " Correcto.", currentUserId, utcNow, asTracking, cancellationToken);
    //        var accessToken = _tokenService.GenerateSessionAccessToken(user.Id, user.SecurityStamp, user.TokenVersion, userSession.UserDeviceId, userSession.Id, utcNow,
    //            _tokenLifetimeSettings.SessionAccessTokenLifetimeMinutes, workProfileId, roleId, campusId);
    //        return Result<RefreshResponse>.Ok(new RefreshResponse(accessToken.Token, accessToken.ExpiresIn, accessToken.ExpiresAt, refreshToken.Identifier, refreshToken.Token, 
    //            refreshToken.ExpiresIn, refreshToken.ExpiresAt));
    //    }
    //}
}