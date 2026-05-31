using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Infrastructure.Abstractions.Http;
using ESAM.GrowTracking.Infrastructure.Abstractions.Validators;
using ESAM.GrowTracking.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class ClientInfoService : IClientInfoService
    {
        private readonly ILogger<ClientInfoService> _logger;
        private readonly IHttpRequestContextReader _requestReader;
        private readonly ReadOnlyCollection<string> _ipHeaderKeys;
        private readonly IIpAddressValidator _ipAddressValidator;

        public ClientInfoService(ILogger<ClientInfoService> logger, IHttpRequestContextReader requestReader, IOptions<ClientInfoSettings> clientInfoSettingsOptions,
            IIpAddressValidator ipAddressValidator)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(requestReader);
            ArgumentNullException.ThrowIfNull(clientInfoSettingsOptions);
            ArgumentNullException.ThrowIfNull(ipAddressValidator);
            var clientInfoSettings = clientInfoSettingsOptions.Value ?? throw new ArgumentNullException(nameof(clientInfoSettingsOptions));
            _logger = logger;
            _requestReader = requestReader;
            _ipHeaderKeys = clientInfoSettings.IpHeaderKeys.Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
            _ipAddressValidator = ipAddressValidator;
        }

        public string? GetIpAddress()
        {
            foreach (var header in _ipHeaderKeys)
            {
                var raw = _requestReader.GetHeaderValue(header);
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var candidate = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
                if (candidate is null)
                    continue;
                if (_ipAddressValidator.TryValidate(candidate, out var ip) && ip is not null)
                    return ip.ToString();
                _logger.LogWarning("GetIpAddress: IP inválida o no pública en el encabezado '{Header}': {Value}", header, raw);
            }
            var remote = _requestReader.GetRemoteIpAddress();
            if (!string.IsNullOrWhiteSpace(remote) && _ipAddressValidator.TryValidate(remote, out var remoteAddr) && remoteAddr is not null)
                return remoteAddr.ToString();
            _logger.LogWarning("GetIpAddress: no se pudo determinar una IP pública válida del cliente.");
            return null;
        }

        public string? GetUserAgent()
        {
            var ua = _requestReader.GetUserAgent();
            if (!string.IsNullOrWhiteSpace(ua))
                return ua;
            _logger.LogWarning("GetUserAgent: el encabezado User-Agent está ausente o vacío.");
            return null;
        }
    }
}