using ESAM.GrowTracking.Application.DTOs;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Security;

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
            int lifetimeMinutes, int? workProfileId = null, int? roleId = null, int? campusId = null)
        {
            ValidateCommonParams(userId, securityStamp, tokenVersion, userDeviceId, lifetimeMinutes);
            if (userSessionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userSessionId), $"{nameof(userSessionId)} debe ser mayor a cero.");
            if (workProfileId is not null && workProfileId <= 0)
                throw new ArgumentOutOfRangeException(nameof(workProfileId), $"{nameof(workProfileId)} debe ser mayor a cero.");
            if (roleId is not null && roleId <= 0)
                throw new ArgumentOutOfRangeException(nameof(roleId), $"{nameof(roleId)} debe ser mayor a cero.");
            if (campusId is not null && campusId <= 0)
                throw new ArgumentOutOfRangeException(nameof(campusId), $"{nameof(campusId)} debe ser mayor a cero.");
            var expiresAt = utcNow.AddMinutes(lifetimeMinutes);
            var jti = Guid.NewGuid().ToString();
            var claims = BuildBaseAccessTokenClaims(jti, userId, securityStamp, tokenVersion, userDeviceId, AccessTokenType.Session, utcNow, expiresAt);
            claims.Add(new Claim(CustomClaimTypes.UserSessionId, userSessionId.ToString(), ClaimValueTypes.Integer64));
            if (workProfileId is not null)
                claims.Add(new Claim(CustomClaimTypes.WorkProfileId, workProfileId.Value.ToString(), ClaimValueTypes.Integer64));
            if (roleId is not null)
                claims.Add(new Claim(CustomClaimTypes.RoleId, roleId.Value.ToString(), ClaimValueTypes.Integer64));
            if (campusId is not null)
                claims.Add(new Claim(CustomClaimTypes.CampusId, campusId.Value.ToString(), ClaimValueTypes.Integer64));
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

        private static List<Claim> BuildBaseAccessTokenClaims(string jti, int userId, string securityStamp, int tokenVersion, int userDeviceId, AccessTokenType accessTokenType,
            DateTime utcNow, DateTime expiresAt)
        {
            return [ new(JwtRegisteredClaimNames.Jti, jti), 
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), 
                new(JwtRegisteredClaimNames.Sub, userId.ToString(), ClaimValueTypes.Integer64), 
                new(CustomClaimTypes.SecurityStamp, securityStamp), 
                new(CustomClaimTypes.TokenVersion, tokenVersion.ToString(), ClaimValueTypes.Integer64), 
                new(CustomClaimTypes.UserDeviceId, userDeviceId.ToString(), ClaimValueTypes.Integer64), 
                new(CustomClaimTypes.AccessTokenType, ((byte)accessTokenType).ToString(), ClaimValueTypes.Integer64), 
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
    }
}