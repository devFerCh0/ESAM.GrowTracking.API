using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Extensions;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class JwtBearerOptionsSetup : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IHostEnvironment _environment;

        public JwtBearerOptionsSetup(IOptions<JwtSettings> jwtSettingsOptions, IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(jwtSettingsOptions);
            ArgumentNullException.ThrowIfNull(environment);
            _jwtSettings = jwtSettingsOptions.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions));
            _environment = environment;
        }

        public void PostConfigure(string? name, JwtBearerOptions jwtBearerOptions)
        {
            if (name != JwtBearerDefaults.AuthenticationScheme)
                return;
            jwtBearerOptions.RequireHttpsMetadata = _environment.IsProduction();
            jwtBearerOptions.SaveToken = false;
            jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            jwtBearerOptions.Events ??= new JwtBearerEvents();
            jwtBearerOptions.Events.OnTokenValidated = async context =>
            {
                var endpoint = context.HttpContext.GetEndpoint();
                var isAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
                if (isAnonymous)
                    return;
                var principal = context.Principal;
                if (principal is null)
                {
                    context.Fail("Token inválido.");
                    return;
                }
                var accessTokenType = principal.GetAccessTokenType();
                var jti = principal.GetJti();
                var userId = principal.GetUserId();
                var securityStamp = principal.GetSecurityStamp();
                var tokenVersion = principal.GetTokenVersion();
                var userDeviceId = principal.GetUserDeviceId();
                var userSessionId = principal.GetUserSessionId();
                var workProfileId = principal.GetWorkProfileId();
                var roleId = principal.GetRoleId();
                var campusId = principal.GetCampusId();
                var accessTokenValidationService = context.HttpContext.RequestServices.GetRequiredService<IAccessTokenValidationService>();
                var ValidateAccessTokenResult = await accessTokenValidationService.ValidateAccessTokenAsync(accessTokenType.Value, jti, userId.Value, securityStamp, 
                    tokenVersion.Value, userDeviceId.Value, userSessionId, workProfileId, roleId, campusId);
                if (ValidateAccessTokenResult.IsFailure)
                {
                    var apiErrors = ValidateAccessTokenResult.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields })
                        .ToList();
                    context.HttpContext.Items["AuthErrors"] = apiErrors;
                    context.Fail("Business Rule Validation Failed");
                }
            };
            jwtBearerOptions.Events.OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                    return;
                await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized.");
            };
            jwtBearerOptions.Events.OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                    return;
                await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Forbidden.");
            };
        }
    }
}