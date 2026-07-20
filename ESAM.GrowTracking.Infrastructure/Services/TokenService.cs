using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Extensions;
using ESAM.GrowTracking.Infrastructure.Security;
using ESAM.GrowTracking.Infrastructure.Settings;
using ESAM.GrowTracking.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly SigningCredentials _signingCredentials;
        private static readonly JsonWebTokenHandler s_jwtHandler = new();

        public TokenService(ILogger<TokenService> logger, IOptions<JwtSettings> jwtSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(jwtSettingsOptions);
            _logger = logger;
            _jwtSettings = jwtSettingsOptions.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));
            _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)), SecurityAlgorithms.HmacSha256);
        }

        public AccessTokenDTO GenerateTemporaryAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, DateTime utcNow,
            int lifetimeMinutes)
        {
            ValidateCommonParams(userId, securityStamp, tokenVersion, userDeviceId, lifetimeMinutes);
            var expiresAt = utcNow.AddMinutes(lifetimeMinutes);
            var jti = Guid.NewGuid().ToString();
            var claims = BuildBaseAccessTokenClaims(jti, userId, securityStamp, tokenVersion, userDeviceId, AccessTokenType.Temporary, utcNow, expiresAt);
            claims.Add(new Claim(CustomClaimTypes.IsPersistent, isPersistent.ToString(), ClaimValueTypes.Boolean));
            return CreateToken(claims, userId, utcNow, expiresAt);
        }

        public AccessTokenDTO GenerateSessionAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, DateTime utcNow,
            int lifetimeMinutes, int workProfileSelectedId, int workProfileId, WorkProfileType workProfileType, int? roleCampusSelectedId = null, int? roleId = null, 
            int? campusId = null)
        {
            ValidateCommonParams(userId, securityStamp, tokenVersion, userDeviceId, lifetimeMinutes);
            if (userSessionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userSessionId), $"{nameof(userSessionId)} debe ser mayor a cero.");
            if (workProfileSelectedId <= 0)
                throw new ArgumentOutOfRangeException(nameof(workProfileSelectedId), $"{nameof(workProfileSelectedId)} debe ser mayor a cero.");
            if (workProfileId <= 0)
                throw new ArgumentOutOfRangeException(nameof(workProfileId), $"{nameof(workProfileId)} debe ser mayor a cero.");
            if (roleCampusSelectedId is not null && roleCampusSelectedId <= 0)
                throw new ArgumentOutOfRangeException(nameof(roleCampusSelectedId), $"{nameof(roleCampusSelectedId)} debe ser mayor a cero.");
            if (roleId is not null && roleId <= 0)
                throw new ArgumentOutOfRangeException(nameof(roleId), $"{nameof(roleId)} debe ser mayor a cero.");
            if (campusId is not null && campusId <= 0)
                throw new ArgumentOutOfRangeException(nameof(campusId), $"{nameof(campusId)} debe ser mayor a cero.");
            var expiresAt = utcNow.AddMinutes(lifetimeMinutes);
            var jti = Guid.NewGuid().ToString();
            var claims = BuildBaseAccessTokenClaims(jti, userId, securityStamp, tokenVersion, userDeviceId, AccessTokenType.Session, utcNow, expiresAt);
            claims.Add(new Claim(CustomClaimTypes.UserSessionId, userSessionId.ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(CustomClaimTypes.WorkProfileSelectedId, workProfileSelectedId.ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(CustomClaimTypes.WorkProfileId, workProfileId.ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(CustomClaimTypes.WorkProfileType, workProfileType.GetStringValue()));
            if (workProfileType == WorkProfileType.WithRoles)
                if (roleCampusSelectedId is not null && roleId is not null && campusId is not null)
                {
                    claims.Add(new Claim(CustomClaimTypes.RoleCampusSelectedId, roleCampusSelectedId.Value.ToString(), ClaimValueTypes.Integer64));
                    claims.Add(new Claim(CustomClaimTypes.RoleId, roleId.Value.ToString(), ClaimValueTypes.Integer64));
                    claims.Add(new Claim(CustomClaimTypes.CampusId, campusId.Value.ToString(), ClaimValueTypes.Integer64));
                }
            return CreateToken(claims, userId, utcNow, expiresAt);
        }

        public RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays)
        {
            if (lifetimeDays <= 0)
                throw new ArgumentOutOfRangeException(nameof(lifetimeDays), $"{nameof(lifetimeDays)} debe ser mayor a cero.");
            var identifier = Guid.NewGuid().ToString("N");
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Base64UrlEncoder.Encode(randomBytes);
            var expiresAt = utcNow.AddDays(lifetimeDays);
            var expiresIn = (int)(expiresAt - utcNow).TotalSeconds;
            return new RefreshTokenDTO(identifier, token, expiresIn, expiresAt);
        }

        public async Task<AccessTokenClaimsDTO> ExtractAccessTokenClaimsAsync(string accessToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingCredentials.Key,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidateLifetime = false,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            };
            TokenValidationResult validationResult;
            try
            {
                validationResult = await s_jwtHandler.ValidateTokenAsync(accessToken, validationParameters);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "El token de acceso proporcionado tiene un formato inválido.");
                throw new SecurityTokenException("El token de acceso proporcionado no es válido.", ex);
            }
            if (!validationResult.IsValid || validationResult.ClaimsIdentity is null)
            {
                _logger.LogWarning(validationResult.Exception, "El token de acceso proporcionado no superó la validación de firma, emisor o audiencia.");
                throw new SecurityTokenException("El token de acceso proporcionado no es válido.", validationResult.Exception);
            }
            return MapToAccessTokenClaims(new ClaimsPrincipal(validationResult.ClaimsIdentity));
        }

        private static List<Claim> BuildBaseAccessTokenClaims(string jti, int userId, string securityStamp, int tokenVersion, int userDeviceId, AccessTokenType accessTokenType,
            DateTime utcNow, DateTime expiresAt)
        {
            return [ new(JwtRegisteredClaimNames.Jti, jti),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Sub, userId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimTypes.SecurityStamp, securityStamp),
                new(CustomClaimTypes.TokenVersion, tokenVersion.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimTypes.UserDeviceId, userDeviceId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimTypes.AccessTokenType, accessTokenType.GetStringValue()),
                new(CustomClaimTypes.AccessTokenExpiration, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ];
        }

        private AccessTokenDTO CreateToken(List<Claim> claims, int userId, DateTime utcNow, DateTime expiresAt)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = utcNow,
                Expires = expiresAt,
                SigningCredentials = _signingCredentials
            };
            try
            {
                var token = s_jwtHandler.CreateToken(descriptor);
                var expiresIn = (int)(expiresAt - utcNow).TotalSeconds;
                return new AccessTokenDTO(token, expiresIn, expiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar JWT para UserId={UserId}.", userId);
                throw new SecurityTokenException("No se pudo generar el token de acceso.", ex);
            }
        }

        private static void ValidateCommonParams(int userId, string securityStamp, int tokenVersion, int userDeviceId, int lifetimeMinutes)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), $"{nameof(userId)} debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(securityStamp))
                throw new ArgumentException($"{nameof(securityStamp)} no puede ser nulo o vacío.", nameof(securityStamp));
            if (tokenVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(tokenVersion), $"{nameof(tokenVersion)} debe ser mayor o igual a cero.");
            if (userDeviceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userDeviceId), $"{nameof(userDeviceId)} debe ser mayor a cero.");
            if (lifetimeMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(lifetimeMinutes), $"{nameof(lifetimeMinutes)} debe ser mayor a cero.");
        }

        private static AccessTokenClaimsDTO MapToAccessTokenClaims(ClaimsPrincipal principal)
        {
            return new AccessTokenClaimsDTO(principal.GetJti(), principal.GetUserId(), principal.GetSecurityStamp(), principal.GetTokenVersion(), principal.GetUserDeviceId(),
                principal.GetAccessTokenType(), principal.GetAccessTokenExpiration(), principal.GetUserSessionId(), principal.GetWorkProfileSelectedId(), 
                principal.GetWorkProfileId(), principal.GetWorkProfileType(), principal.GetRoleCampusSelectedId(), principal.GetRoleId(), principal.GetCampusId());
        }
    }

    //public sealed class TokenService : ITokenService
    //{
    //    private readonly ILogger<TokenService> _logger;
    //    private readonly JwtSettings _jwtSettings;
    //    private readonly SigningCredentials _signingCredentials;
    //    private static readonly JsonWebTokenHandler s_jwtHandler = new();

    //    public TokenService(ILogger<TokenService> logger, IOptions<JwtSettings> jwtSettingsOptions)
    //    {
    //        ArgumentNullException.ThrowIfNull(logger);
    //        ArgumentNullException.ThrowIfNull(jwtSettingsOptions);
    //        _logger = logger;
    //        _jwtSettings = jwtSettingsOptions.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));
    //        _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)), SecurityAlgorithms.HmacSha256);
    //    }

    //    public AccessTokenDTO GenerateTemporaryAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, DateTime utcNow,
    //        int lifetimeMinutes)
    //    {
    //        ValidateCommonParams(userId, securityStamp, tokenVersion, userDeviceId, lifetimeMinutes);
    //        var expiresAt = utcNow.AddMinutes(lifetimeMinutes);
    //        var jti = Guid.NewGuid().ToString();
    //        var claims = BuildBaseAccessTokenClaims(jti, userId, securityStamp, tokenVersion, userDeviceId, AccessTokenType.Temporary, utcNow, expiresAt);
    //        claims.Add(new Claim(CustomClaimTypes.IsPersistent, isPersistent.ToString(), ClaimValueTypes.Boolean));
    //        return CreateToken(claims, userId, utcNow, expiresAt);
    //    }

    //    public AccessTokenDTO GenerateSessionAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, DateTime utcNow,
    //        int lifetimeMinutes, int workProfileId, WorkProfileType workProfileType, int? roleId = null, int? campusId = null)
    //    {
    //        ValidateCommonParams(userId, securityStamp, tokenVersion, userDeviceId, lifetimeMinutes);
    //        if (userSessionId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(userSessionId), $"{nameof(userSessionId)} debe ser mayor a cero.");
    //        if (workProfileId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(workProfileId), $"{nameof(workProfileId)} debe ser mayor a cero.");
    //        if (roleId is not null && roleId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(roleId), $"{nameof(roleId)} debe ser mayor a cero.");
    //        if (campusId is not null && campusId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(campusId), $"{nameof(campusId)} debe ser mayor a cero.");
    //        var expiresAt = utcNow.AddMinutes(lifetimeMinutes);
    //        var jti = Guid.NewGuid().ToString();
    //        var claims = BuildBaseAccessTokenClaims(jti, userId, securityStamp, tokenVersion, userDeviceId, AccessTokenType.Session, utcNow, expiresAt);
    //        claims.Add(new Claim(CustomClaimTypes.UserSessionId, userSessionId.ToString(), ClaimValueTypes.Integer64));
    //        claims.Add(new Claim(CustomClaimTypes.WorkProfileId, workProfileId.ToString(), ClaimValueTypes.Integer64));
    //        claims.Add(new Claim(CustomClaimTypes.WorkProfileType, workProfileType.GetStringValue()));
    //        if (workProfileType == WorkProfileType.WithRoles)
    //            if (roleId is not null && campusId is not null)
    //            {
    //                claims.Add(new Claim(CustomClaimTypes.RoleId, roleId.Value.ToString(), ClaimValueTypes.Integer64));
    //                claims.Add(new Claim(CustomClaimTypes.CampusId, campusId.Value.ToString(), ClaimValueTypes.Integer64));
    //            }
    //        return CreateToken(claims, userId, utcNow, expiresAt);
    //    }

    //    public RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays)
    //    {
    //        if (lifetimeDays <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(lifetimeDays), $"{nameof(lifetimeDays)} debe ser mayor a cero.");
    //        var identifier = Guid.NewGuid().ToString("N");
    //        var randomBytes = RandomNumberGenerator.GetBytes(64);
    //        var token = Base64UrlEncoder.Encode(randomBytes);
    //        var expiresAt = utcNow.AddDays(lifetimeDays);
    //        var expiresIn = (int)(expiresAt - utcNow).TotalSeconds;
    //        return new RefreshTokenDTO(identifier, token, expiresIn, expiresAt);
    //    }

    //    public async Task<AccessTokenClaimsDTO> ExtractAccessTokenClaimsAsync(string accessToken)
    //    {
    //        var validationParameters = new TokenValidationParameters
    //        {
    //            ValidateIssuer = true,
    //            ValidIssuer = _jwtSettings.Issuer,
    //            ValidateAudience = true,
    //            ValidAudience = _jwtSettings.Audience,
    //            ValidateIssuerSigningKey = true,
    //            IssuerSigningKey = _signingCredentials.Key,
    //            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
    //            ValidateLifetime = false,
    //            RequireExpirationTime = true,
    //            ClockSkew = TimeSpan.Zero
    //        };
    //        TokenValidationResult validationResult;
    //        try
    //        {
    //            validationResult = await s_jwtHandler.ValidateTokenAsync(accessToken, validationParameters);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogWarning(ex, "El token de acceso proporcionado tiene un formato inválido.");
    //            throw new SecurityTokenException("El token de acceso proporcionado no es válido.", ex);
    //        }
    //        if (!validationResult.IsValid || validationResult.ClaimsIdentity is null)
    //        {
    //            _logger.LogWarning(validationResult.Exception, "El token de acceso proporcionado no superó la validación de firma, emisor o audiencia.");
    //            throw new SecurityTokenException("El token de acceso proporcionado no es válido.", validationResult.Exception);
    //        }
    //        return MapToAccessTokenClaims(new ClaimsPrincipal(validationResult.ClaimsIdentity));
    //    }

    //    private static List<Claim> BuildBaseAccessTokenClaims(string jti, int userId, string securityStamp, int tokenVersion, int userDeviceId, AccessTokenType accessTokenType,
    //        DateTime utcNow, DateTime expiresAt)
    //    {
    //        return [ new(JwtRegisteredClaimNames.Jti, jti), 
    //            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), 
    //            new(JwtRegisteredClaimNames.Sub, userId.ToString(), ClaimValueTypes.Integer64), 
    //            new(CustomClaimTypes.SecurityStamp, securityStamp), 
    //            new(CustomClaimTypes.TokenVersion, tokenVersion.ToString(), ClaimValueTypes.Integer64), 
    //            new(CustomClaimTypes.UserDeviceId, userDeviceId.ToString(), ClaimValueTypes.Integer64),
    //            new(CustomClaimTypes.AccessTokenType, accessTokenType.GetStringValue()),
    //            new(CustomClaimTypes.AccessTokenExpiration, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    //        ];
    //    }

    //    private AccessTokenDTO CreateToken(List<Claim> claims, int userId, DateTime utcNow, DateTime expiresAt)
    //    {
    //        var descriptor = new SecurityTokenDescriptor
    //        {
    //            Issuer = _jwtSettings.Issuer,
    //            Audience = _jwtSettings.Audience,
    //            Subject = new ClaimsIdentity(claims),
    //            NotBefore = utcNow,
    //            Expires = expiresAt,
    //            SigningCredentials = _signingCredentials
    //        };
    //        try
    //        {
    //            var token = s_jwtHandler.CreateToken(descriptor);
    //            var expiresIn = (int)(expiresAt - utcNow).TotalSeconds;
    //            return new AccessTokenDTO(token, expiresIn, expiresAt);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error al generar JWT para UserId={UserId}.", userId);
    //            throw new SecurityTokenException("No se pudo generar el token de acceso.", ex);
    //        }
    //    }

    //    private static void ValidateCommonParams(int userId, string securityStamp, int tokenVersion, int userDeviceId, int lifetimeMinutes)
    //    {
    //        if (userId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(userId), $"{nameof(userId)} debe ser mayor a cero.");
    //        if (string.IsNullOrWhiteSpace(securityStamp))
    //            throw new ArgumentException($"{nameof(securityStamp)} no puede ser nulo o vacío.", nameof(securityStamp));
    //        if (tokenVersion < 0)
    //            throw new ArgumentOutOfRangeException(nameof(tokenVersion), $"{nameof(tokenVersion)} debe ser mayor o igual a cero.");
    //        if (userDeviceId <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(userDeviceId), $"{nameof(userDeviceId)} debe ser mayor a cero.");
    //        if (lifetimeMinutes <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(lifetimeMinutes), $"{nameof(lifetimeMinutes)} debe ser mayor a cero.");
    //    }

    //    private static AccessTokenClaimsDTO MapToAccessTokenClaims(ClaimsPrincipal principal)
    //    {
    //        return new AccessTokenClaimsDTO(principal.GetJti(), principal.GetUserId(), principal.GetSecurityStamp(), principal.GetTokenVersion(), principal.GetUserDeviceId(),
    //            principal.GetAccessTokenType(), principal.GetAccessTokenExpiration(), principal.GetUserSessionId(), principal.GetWorkProfileId(), principal.GetWorkProfileType(), 
    //            principal.GetRoleId(), principal.GetCampusId());
    //    }
    //}
}