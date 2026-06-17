using ESAM.GrowTracking.API.Abstractions.Security;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
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
                var handler = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenValidatedHandler>();
                var result = await handler.HandleAsync(context.Principal, context.HttpContext.RequestAborted);
                if (result.IsFailure)
                {
                    var authErrors = result.Errors.Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields }).ToList();
                    context.HttpContext.Items[JwtAuthenticationItemKeys.AuthErrors] = authErrors;
                    context.Fail("Token validation failed.");
                    //context.HttpContext.Items[JwtAuthenticationItemKeys.AuthErrors] = result.Errors; // Almacena IReadOnlyList<Error> (Application); la conversión a ApiErrorItem ocurre en OnChallenge
                    //context.Fail("Token validation failed.");
                }
            };
            jwtBearerOptions.Events.OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                    return;
                if (context.HttpContext.Items.TryGetValue(JwtAuthenticationItemKeys.AuthErrors, out var stored) && stored is IReadOnlyList<ApiErrorItem> authErrors
                && authErrors.Count > 0)
                {
                    await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, authErrors);
                    return;
                }
                await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized.");
                //context.HandleResponse();
                //if (context.Response.HasStarted)
                //    return;
                //if (context.HttpContext.Items.TryGetValue(JwtAuthenticationItemKeys.AuthErrors, out var stored)
                //    && stored is IReadOnlyList<Error> authErrors && authErrors.Count > 0)
                //{
                //    var errorItems = authErrors // Conversión Error → ApiErrorItem centralizada aquí, en la capa HTTP
                //        .Select(e => new ApiErrorItem { Message = e.Message, Fields = e.Fields })
                //        .ToList();
                //    await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, errorItems);
                //    return;
                //}
                //await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized.");
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