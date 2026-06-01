using ESAM.GrowTracking.API.Responses;
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