using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public record RefreshCommand : IRequest<Result<RefreshResponse>>
    {
        public string AccessToken { get; init; }

        public string RefreshTokenRaw { get; init; }

        public RefreshCommand(string accessToken, string refreshTokenRaw)
        {
            AccessToken = accessToken;
            RefreshTokenRaw = refreshTokenRaw;
        }
    }
}