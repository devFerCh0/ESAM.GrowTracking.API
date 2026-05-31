using System.Security.Claims;

namespace ESAM.GrowTracking.Infrastructure.Abstractions.Http
{
    public interface IClaimsPrincipalProvider
    {
        ClaimsPrincipal? Current { get; }
    }
}