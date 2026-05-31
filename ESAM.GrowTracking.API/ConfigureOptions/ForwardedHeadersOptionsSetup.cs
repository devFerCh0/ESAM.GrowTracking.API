using ESAM.GrowTracking.API.Settings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Net;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class ForwardedHeadersOptionsSetup : IConfigureOptions<ForwardedHeadersOptions>
    {
        private readonly ForwardedHeadersSettings _forwardedHeadersSettings;

        public ForwardedHeadersOptionsSetup(IOptions<ForwardedHeadersSettings> forwardedHeadersSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(forwardedHeadersSettingsOptions);
            _forwardedHeadersSettings = forwardedHeadersSettingsOptions.Value ?? throw new ArgumentNullException(nameof(forwardedHeadersSettingsOptions));
        }

        public void Configure(ForwardedHeadersOptions options)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var cidr in _forwardedHeadersSettings.KnownNetworks)
            {
                var parts = cidr.Split('/', 2);
                var ip = IPAddress.Parse(parts[0]);
                var prefix = int.Parse(parts[1]);
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(ip, prefix));
            }
            foreach (var proxy in _forwardedHeadersSettings.KnownProxies)
                options.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
}