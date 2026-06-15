using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Extensions;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                var principal = context.Principal;
                if (principal == null)
                {
                    context.Fail("Token inválido.");
                    return;
                }
                var jti = principal.GetJti();
                var userId = principal.GetUserId();
                var securityStamp = principal.GetSecurityStamp();
                var tokenVersion = principal.GetTokenVersion();
                var userDeviceId = principal.GetUserDeviceId();
                if (string.IsNullOrEmpty(jti) || userId is null || string.IsNullOrEmpty(securityStamp) || tokenVersion is null || userDeviceId is null)
                {
                    context.HttpContext.Items["AuthError"] = "Faltan claims obligatorios.";
                    context.Fail("Claims faltantes");
                    return;
                }
                var sessionValidationService = context.HttpContext.RequestServices.GetRequiredService<ICurrentSessionValidationService>();

                var userValidationResult = await sessionValidationService.ValidateCurrentUserAsync(userId.Value, securityStamp, tokenVersion.Value, DateTime.UtcNow);
                if (userValidationResult.IsFailure)
                {
                    var apiErrors = userValidationResult.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields }).ToList();
                    context.HttpContext.Items["AuthErrors"] = apiErrors;
                    context.Fail("User Validation Failed");
                    return;
                }

                var deviceValidationResult = await sessionValidationService.ValidateCurrentUserDeviceAsync(userDeviceId.Value, userId.Value, DateTime.UtcNow);
                if (deviceValidationResult.IsFailure)
                {
                    var apiErrors = deviceValidationResult.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields }).ToList();
                    context.HttpContext.Items["AuthErrors"] = apiErrors;
                    context.Fail("Device Validation Failed");
                    return;
                }

                var accesstokenValidationResult = await sessionValidationService.ValidateCurrentAccessTokenTemporaryAsync(jti);
                if (accesstokenValidationResult.IsFailure)
                {
                    var apiErrors = accesstokenValidationResult.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields }).ToList();
                    context.HttpContext.Items["AuthErrors"] = apiErrors;
                    context.Fail("Token Inválido");
                    return;
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