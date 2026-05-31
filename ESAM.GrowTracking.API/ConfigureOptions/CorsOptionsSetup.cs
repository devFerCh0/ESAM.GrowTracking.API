using ESAM.GrowTracking.Infrastructure.Abstractions.Validators;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class CorsOptionsSetup : IConfigureOptions<CorsOptions>
    {
        private readonly ICorsOriginValidator _corsOriginValidator;
        private readonly CorsSettings _corsSettings;

        public CorsOptionsSetup(ICorsOriginValidator corsOriginValidator, IOptions<CorsSettings> corsSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(corsOriginValidator);
            ArgumentNullException.ThrowIfNull(corsSettingsOptions);
            _corsOriginValidator = corsOriginValidator;
            _corsSettings = corsSettingsOptions.Value ?? throw new ArgumentNullException(nameof(corsSettingsOptions));
        }

        public void Configure(CorsOptions options)
        {
            var policyName = string.IsNullOrWhiteSpace(_corsSettings.PolicyName) ? "CorsPolicy" : _corsSettings.PolicyName;
            options.AddDefaultPolicy(ApplyPolicySettings);
            options.AddPolicy(policyName, ApplyPolicySettings);
        }

        private void ApplyPolicySettings(CorsPolicyBuilder builder)
        {
            builder.SetIsOriginAllowed(_corsOriginValidator.IsOriginAllowed);
            if (_corsSettings.AllowedMethods?.Count > 0)
                builder.WithMethods([.. _corsSettings.AllowedMethods]);
            else
                builder.AllowAnyMethod();
            if (_corsSettings.AllowedHeaders?.Count > 0)
                builder.WithHeaders([.. _corsSettings.AllowedHeaders]);
            else
                builder.AllowAnyHeader();
            if (_corsSettings.ExposeHeaders?.Count > 0)
                builder.WithExposedHeaders([.. _corsSettings.ExposeHeaders]);
            if (_corsSettings.AllowCredentials)
                builder.AllowCredentials();
            else
                builder.DisallowCredentials();
            builder.SetPreflightMaxAge(TimeSpan.FromSeconds(Math.Max(0, _corsSettings.PreflightMaxAgeSeconds)));
        }
    }
}