using ESAM.GrowTracking.Application.Results;
using System.Security.Claims;

namespace ESAM.GrowTracking.API.Abstractions.Security
{
    public interface IJwtTokenValidatedHandler
    {
        Task<Result> HandleAsync(ClaimsPrincipal? principal, CancellationToken cancellationToken = default);
    }
}