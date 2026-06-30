using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Extensions;
using ESAM.GrowTracking.Application.Helpers;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly ILogger<LogoutCommandHandler> _logger;
        private readonly IValidator<LogoutCommand> _validator;
        private readonly IAccessTokenClaimsValidatorService _accessTokenClaimsValidatorService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokenRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHashService _hashService;

        public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger, IValidator<LogoutCommand> validator, IAccessTokenClaimsValidatorService accessTokenClaimsValidatorService,
            IDateTimeService dateTimeService, IUnitOfWork unitOfWork, IUserSessionRefreshTokenRepository userSessionRefreshTokenRepository,
            IUserSessionRepository userSessionRepository, IUserSessionService userSessionService, IHashService hashService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(accessTokenClaimsValidatorService);
            ArgumentNullException.ThrowIfNull(dateTimeService);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokenRepository);
            ArgumentNullException.ThrowIfNull(userSessionRepository);
            ArgumentNullException.ThrowIfNull(userSessionService);
            ArgumentNullException.ThrowIfNull(hashService);
            _logger = logger;
            _validator = validator;
            _accessTokenClaimsValidatorService = accessTokenClaimsValidatorService;
            _dateTimeService = dateTimeService;
            _unitOfWork = unitOfWork;
            _userSessionRefreshTokenRepository = userSessionRefreshTokenRepository;
            _userSessionRepository = userSessionRepository;
            _userSessionService = userSessionService;
            _hashService = hashService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validation.ToCommandErrors());
            }
            var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
            var asTracking = false;
            var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
            var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
            var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
            var utcNow = _dateTimeService.UtcNow;
            var revokedReasonPrefix = "Cerrar Sesión:";
            // CUANDO EL CLAIM CURRENTACCESSTOKENTYPE DEL ACCESS TOKEN ES DE TIPO TEMPORARY: NO SE TIENE SESION DE USUARIO, POR LO TANTO TAMPOCO SE TIENE REFRESH TOKEN
            if (currentAccessTokenType == AccessTokenType.Temporary)
            {
                if (request.RefreshTokenRaw is not null)
                {
                    RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var tokenPlain);
                    if (identifier is not null)
                    {
                        var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
                        if (userSessionRefreshToken is not null)
                        {
                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                            if (mismatchedUserSession is not null)
                            {
                                //Introducir logger y determinar mensaje del logger
                                //Determinar razón del por que se está revocando el token de acceso temporal
                                //Modificar la razón del por que se está revocando la sesion
                                //REVOCACION ANOMALA
                                await _userSessionService.RevokeUserSessionAndAccessTokenTemporaryAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                    $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId,
                                    utcNow, asTracking, cancellationToken);
                                return Result.Ok();
                            }
                            //Introducir logger y determinar mensaje del logger
                            //Determinar razón del por que se está revocando el token de acceso temporal
                            //REVOCACION ANOMALA
                        }
                        else
                        {
                            //Introducir logger y determinar mensaje del logger
                            //Determinar razón del por que se está revocando el token de acceso temporal
                            //REVOCACION ANOMALA
                        }
                    }
                    else
                    {
                        //Introducir logger y determinar mensaje del logger
                        //Determinar razón del por que se está revocando el token de acceso temporal
                        //REVOCACION ANOMALA
                    }
                }
                else
                {
                    //Introducir logger y determinar mensaje del logger
                    //Determinar razón del por que se está revocando el token de acceso temporal
                    //REVOCACION CORRECTA
                }
                var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, currentJti, currentAccessTokenExpiration, utcNow, 
                    $"{revokedReasonPrefix} Access token temporal revocado.", currentUserId, utcNow);
                await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok();
            }
            // CUANDO EL CLAIM CURRENTACCESSTOKENTYPE DEL ACCESS TOKEN ES DE TIPO SESSION: SI SE TIENE SESION DE USUARIO, POR LO TANTO TAMBIEN SE TIENE REFRESH TOKEN
            RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier1, out var tokenPlain1);
            var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
            var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
            if (identifier1 is not null)
            {
                var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier1, asTracking, cancellationToken);
                if (userSessionRefreshToken is null)
                {
                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                    if (mismatchedUserSession is not null)
                    {
                        //Introducir logger y determinar mensaje del logger
                        //Determinar razón del por que se está revocando el token de acceso session
                        //Modificar la razón del por que se está revocando la sesion
                        //REVOCACION ANOMALA
                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                            $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow, 
                            asTracking, cancellationToken);
                        return Result.Ok();
                    }
                    //Introducir logger y determinar mensaje del logger
                    //Determinar razón del por que se está revocando el token de acceso session
                    //REVOCACION ANOMALA
                }
                else if (userSessionRefreshToken.UserSessionId != currentUserSessionId)
                {
                    var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
                    var mismatchedUserSession1 = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                    if (mismatchedUserSession is not null && mismatchedUserSession1 is null)
                    {
                        //Introducir logger y determinar mensaje del logger
                        //Determinar razón del por que se está revocando el token de acceso session
                        //Modificar la razón del por que se está revocando la sesion
                        //REVOCACION ANOMALA
                        await _userSessionService.RevokeUserSessionAsync(mismatchedUserSession, 
                            $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow,
                            asTracking, cancellationToken);
                        return Result.Ok();
                    }
                    if (mismatchedUserSession is null && mismatchedUserSession1 is not null)
                    {
                        //Introducir logger y determinar mensaje del logger
                        //Determinar razón del por que se está revocando el token de acceso session
                        //Modificar la razón del por que se está revocando la sesion
                        //REVOCACION ANOMALA
                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession1, currentJti, currentAccessTokenExpiration,
                            $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow,
                            asTracking, cancellationToken);
                        return Result.Ok();
                    }
                    if (mismatchedUserSession is not null && mismatchedUserSession1 is not null)
                    {
                        //Introducir logger y determinar mensaje del logger
                        //Determinar razón del por que se está revocando el token de acceso session
                        //Modificar la razón del por que se está revocando la sesion
                        //REVOCACION ANOMALA
                        await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, mismatchedUserSession1, currentJti, 
                            currentAccessTokenExpiration, 
                            $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow,
                            asTracking, cancellationToken);
                        return Result.Ok();
                    }
                    if (mismatchedUserSession is null && mismatchedUserSession1 is null)
                    {
                        //Introducir logger y determinar mensaje del logger
                    }
                    ////Introducir logger y determinar mensaje del logger
                    ////Determinar razón del por que se está revocando el token de acceso session
                    ////REVOCACION ANOMALA
                }
                else
                {
                    if (tokenPlain1 is null)
                    {
                        var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                        if (mismatchedUserSession is not null)
                        {
                            //Introducir logger y determinar mensaje del logger
                            //Determinar razón del por que se está revocando el token de acceso session
                            //Modificar la razón del por que se está revocando la sesion
                            //REVOCACION ANOMALA
                            await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow,
                                asTracking, cancellationToken);
                            return Result.Ok();
                        }
                        else
                        {
                            //Introducir logger y determinar mensaje del logger
                        }
                        ////Introducir logger y determinar mensaje del logger
                        ////Determinar razón del por que se está revocando el token de acceso session
                        ////REVOCACION ANOMALA
                    }
                    else
                    {
                        var computedHash = _hashService.ComputeHash(tokenPlain1, userSessionRefreshToken.Salt);
                        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash)))
                        {
                            var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                            if (mismatchedUserSession is not null)
                            {
                                //Introducir logger y determinar mensaje del logger
                                //Determinar razón del por que se está revocando el token de acceso session
                                //Modificar la razón del por que se está revocando la sesion
                                //REVOCACION ANOMALA
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                                    $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, 
                                    utcNow, asTracking, cancellationToken);
                                return Result.Ok();
                            }
                            else
                            {
                                //Introducir logger y determinar mensaje del logger
                            }
                            ////Introducir logger y determinar mensaje del logger
                            ////Determinar razón del por que se está revocando el token de acceso session
                            ////REVOCACION ANOMALA
                        }
                        else
                        {
                            var userSessionToRevoke = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, 
                                asTracking, cancellationToken);
                            if (userSessionToRevoke is not null)
                            {
                                //Introducir logger y determinar mensaje del logger
                                //Determinar razón del por que se está revocando el token de acceso session
                                //Modificar la razón del por que se está revocando la sesion
                                //REVOCACION CORRECTA
                                await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(userSessionToRevoke, currentJti, currentAccessTokenExpiration,
                                    $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId,
                                    utcNow, asTracking, cancellationToken);
                                return Result.Ok();
                            }
                            else
                            {
                                //Introducir logger y determinar mensaje del logger
                            }
                            ////Introducir logger y determinar mensaje del logger
                            ////Determinar razón del por que se está revocando el token de acceso session
                            ////REVOCACION ANOMALA
                        }
                    }
                }
            }
            else
            {
                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(currentUserSessionId, asTracking, cancellationToken);
                if (mismatchedUserSession is not null)
                {
                    //Introducir logger y determinar mensaje del logger
                    //Determinar razón del por que se está revocando el token de acceso session
                    //Modificar la razón del por que se está revocando la sesion
                    //REVOCACION ANOMALA
                    await _userSessionService.RevokeUserSessionAndAccessTokenSessionAsync(mismatchedUserSession, currentJti, currentAccessTokenExpiration,
                        $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId,
                        utcNow, asTracking, cancellationToken);
                    return Result.Ok();
                }
                else
                {
                    //Introducir logger y determinar mensaje del logger
                }
                ////Introducir logger y determinar mensaje del logger
                ////Determinar razón del por que se está revocando el token de acceso session
                ////REVOCACION ANOMALA
            }
            var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow,
                $"{revokedReasonPrefix} Access token temporal revocado.", currentUserId, utcNow);
            await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        //public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        //{
        //    var currentAccessTokenType = _accessTokenClaimsValidatorService.CurrentAccessTokenType;
        //    var currentUserId = _accessTokenClaimsValidatorService.CurrentUserId;
        //    var currentJti = _accessTokenClaimsValidatorService.CurrentJti;
        //    var currentAccessTokenExpiration = _accessTokenClaimsValidatorService.CurrentAccessTokenExpiration;
        //    var utcNow = _dateTimeService.UtcNow;
        //    var revokedReasonPrefix = "Cerrar Sesión:";
        //    if (currentAccessTokenType == AccessTokenType.Temporary)
        //    {
        //        var blacklistedAccessTokenTemporary = new BlacklistedAccessTokenTemporary(currentUserId, currentJti, currentAccessTokenExpiration, utcNow,
        //            $"{revokedReasonPrefix} Access token temporal revocado.", currentUserId, utcNow);
        //        await _unitOfWork.BlacklistedAccessTokensTemporary.InsertAsync(blacklistedAccessTokenTemporary, cancellationToken);
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);
        //        return Result.Ok();
        //    }
        //    var validation = await _validator.ValidateAsync(request, cancellationToken);
        //    if (!validation.IsValid)
        //    {
        //        _logger.LogWarning("LogoutCommand: Validación fallida. Errores: {Errors}", string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)));
        //        return Result.Fail(validation.ToCommandErrors());
        //    }
        //    RefreshTokenParser.TryParse(request.RefreshTokenRaw, out var identifier, out var tokenPlain);

        //    var asTracking = false;
        //    string finalRevokedReason = string.Empty;
        //    UserSession? userSessionToRevoke = null;
        //    var skipFallbackQuery = false;
        //    var currentUserSessionId = _accessTokenClaimsValidatorService.CurrentUserSessionId;
        //    var currentUserDeviceId = _accessTokenClaimsValidatorService.CurrentUserDeviceId;
        //    if (identifier is not null && tokenPlain is not null)
        //    {
        //        var userSessionRefreshToken = await _userSessionRefreshTokenRepository.GetByIdentifierAsync(identifier, asTracking, cancellationToken);
        //        if (userSessionRefreshToken is null)
        //        {
        //            /*
        //             * Obtebemos sesion de usuario por claims
        //             * validamos
        //             * Revocamos
        //             * */
        //            _logger.LogWarning("LogoutCommand: RT provisto no encontrado en BD. UserId={UserId}, Identifier={Identifier}", currentUserId, identifier);
        //            finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT (RT no encontrado).";
        //        }
        //        else if (userSessionRefreshToken.UserSessionId != currentUserSessionId)
        //        {
        //            /*
        //             * Obtenemos sesion de usaurio por userSessionRefreshToken.UserSessionId
        //             * Obtenemos sesion de usuario por claims
        //             * validamos
        //             * revocamos
        //             * */
        //            var computedHashForMismatch = _hashService.ComputeHash(tokenPlain, userSessionRefreshToken.Salt);
        //            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHashForMismatch), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash)))
        //            {
        //                _logger.LogWarning("LogoutCommand: Hash del RT no verificó en mismatch AT-RT. Posible enumeración de identifier. UserId={UserId}, Identifier={Identifier}", currentUserId, identifier);
        //                finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT; hash del RT no verificó en mismatch AT-RT (alerta de seguridad).";
        //            }
        //            else
        //            {
        //                _logger.LogWarning("LogoutCommand: Discrepancia crítica binding AT-RT. UserId={UserId}, AT_Session={ATSession}, RT_Session={RTSession}", currentUserId,
        //                    currentUserSessionId, userSessionRefreshToken.UserSessionId);
        //                var mismatchedUserSession = await _userSessionRepository.GetByIdAsync(userSessionRefreshToken.UserSessionId, asTracking, cancellationToken);
        //                if (mismatchedUserSession is not null)
        //                {
        //                    await _userSessionService.RevokeUserSessionAsync(mismatchedUserSession, null, null,
        //                        $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión vinculada al RT recibido revocada por prevención de seguridad.", currentUserId, utcNow,
        //                        asTracking, cancellationToken);
        //                    finalRevokedReason = $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión del RT revocada. Sesión propia revocada por prevención de seguridad.";
        //                }
        //                else
        //                    finalRevokedReason = $"{revokedReasonPrefix} Binding AT-RT no coincidente; sesión del RT no encontrada. Sesión propia revocada por prevención de seguridad.";
        //            }
        //        }
        //        else
        //        {
        //            var computedHash = _hashService.ComputeHash(tokenPlain!, userSessionRefreshToken.Salt);
        //            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(userSessionRefreshToken.TokenHash)))
        //            {
        //                _logger.LogWarning("LogoutCommand: Hash del RT no verificó. Posible uso indebido del identifier. UserId={UserId}, Identifier={Identifier}", currentUserId,
        //                    identifier);
        //                finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT; hash del RT no verificó (alerta de seguridad).";
        //            }
        //            else
        //            {
        //                userSessionToRevoke = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, 
        //                    asTracking, cancellationToken);
        //                if (userSessionToRevoke is null)
        //                {
        //                    _logger.LogWarning("LogoutCommand: Sesión no localizable por criterios de propiedad tras binding AT-RT verificado. " + 
        //                        "UserId={UserId}, UserSessionId={UserSessionId}", currentUserId, currentUserSessionId);
        //                    skipFallbackQuery = true;
        //                    var blacklistedAccessTokenSession = new BlacklistedAccessTokenSession(currentUserSessionId, currentJti, currentAccessTokenExpiration, utcNow,
        //                        $"{revokedReasonPrefix} Sesión no encontrada por criterios de propiedad (AT-RT verificado); JTI invalidado como medida de seguridad.", 
        //                        currentUserId, utcNow);
        //                    await _unitOfWork.BlacklistedAccessTokensSession.InsertAsync(blacklistedAccessTokenSession, cancellationToken);
        //                    await _unitOfWork.SaveChangesAsync(cancellationToken);
        //                }
        //                else
        //                    finalRevokedReason = $"{revokedReasonPrefix} Logout exitoso con binding AT-RT verificado.";
        //            }
        //        }
        //    }




        //    else
        //        finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT (RT no provisto).";
        //    if (userSessionToRevoke is null && !skipFallbackQuery)
        //    {
        //        userSessionToRevoke = await _userSessionRepository.GetByIdAndUserIdAndUserDeviceIdAsync(currentUserSessionId, currentUserId, currentUserDeviceId, asTracking,
        //            cancellationToken);
        //        if (string.IsNullOrEmpty(finalRevokedReason))
        //            finalRevokedReason = $"{revokedReasonPrefix} Logout ejecutado por claims del AT.";
        //    }
        //    if (userSessionToRevoke is not null)
        //        await _userSessionService.RevokeUserSessionAsync(userSessionToRevoke, currentJti, currentAccessTokenExpiration, finalRevokedReason, currentUserId, utcNow,
        //            asTracking, cancellationToken);
        //    else
        //        _logger.LogWarning("LogoutCommand: Logout completado sin revocación efectiva. AT válido pero sesión no localizable. UserId={UserId}, UserSessionId={UserSessionId}",
        //            currentUserId, currentUserSessionId);
        //    return Result.Ok();
        //}
    }
}