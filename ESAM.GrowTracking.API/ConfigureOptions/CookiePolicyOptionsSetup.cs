using ESAM.GrowTracking.Infrastructure.Http;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class CookiePolicyOptionsSetup : IPostConfigureOptions<CookiePolicyOptions>
    {
        private readonly CookieSettings _cookieSettings;

        public CookiePolicyOptionsSetup(IOptions<CookieSettings> cookieSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(cookieSettingsOptions);
            _cookieSettings = cookieSettingsOptions.Value ?? throw new ArgumentNullException(nameof(cookieSettingsOptions));
        }

        public void PostConfigure(string? name, CookiePolicyOptions options)
        {
            if (name != Options.DefaultName)
                return;
            options.HttpOnly = HttpOnlyPolicy.None;
            options.MinimumSameSitePolicy = MapSameSiteMode(_cookieSettings.SameSite);
            options.Secure = _cookieSettings.UseHostPrefix || _cookieSettings.AlwaysSecure ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
            options.OnAppendCookie = EnforceHostPrefixConstraints;
            options.OnDeleteCookie = EnforceHostPrefixConstraints;
        }

        private void EnforceHostPrefixConstraints(AppendCookieContext ctx)
        {
            if (_cookieSettings.UseHostPrefix && ctx.CookieName.StartsWith("__Host-", StringComparison.Ordinal))
            {
                ctx.CookieOptions.Secure = true;
                ctx.CookieOptions.Path = "/";
                ctx.CookieOptions.Domain = null;
            }
        }

        private void EnforceHostPrefixConstraints(DeleteCookieContext ctx)
        {
            if (_cookieSettings.UseHostPrefix && ctx.CookieName.StartsWith("__Host-", StringComparison.Ordinal))
            {
                ctx.CookieOptions.Secure = true;
                ctx.CookieOptions.Path = "/";
                ctx.CookieOptions.Domain = null;
            }
        }

        private static SameSiteMode MapSameSiteMode(SameSitePolicy policy) => policy switch
        {
            SameSitePolicy.None => SameSiteMode.None,
            SameSitePolicy.Lax => SameSiteMode.Lax,
            SameSitePolicy.Strict => SameSiteMode.Strict,
            SameSitePolicy.Unspecified => SameSiteMode.Unspecified,
            _ => SameSiteMode.Lax
        };
    }
}