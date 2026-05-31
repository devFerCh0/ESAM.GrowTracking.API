using ESAM.GrowTracking.Infrastructure.Abstractions.Validators;
using ESAM.GrowTracking.Infrastructure.Extensions;
using System.Net;

namespace ESAM.GrowTracking.Infrastructure.Validators
{
    public sealed class IpAddressValidator : IIpAddressValidator
    {
        public bool TryValidate(string candidate, out IPAddress? address)
        {
            address = null;
            if (string.IsNullOrWhiteSpace(candidate))
                return false;
            if (!IPAddress.TryParse(candidate, out var parsed))
                return false;
            if (parsed.IsIPv4MappedToIPv6)
                parsed = parsed.MapToIPv4();
            if (parsed.IsLoopback() || parsed.IsPrivate() || parsed.IsReserved())
                return false;
            address = parsed;
            return true;
        }
    }
}