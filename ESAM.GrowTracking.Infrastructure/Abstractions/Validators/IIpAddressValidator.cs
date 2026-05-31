using System.Net;

namespace ESAM.GrowTracking.Infrastructure.Abstractions.Validators
{
    public interface IIpAddressValidator
    {
        bool TryValidate(string candidate, out IPAddress? address);
    }
}