using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.Logout
{
    public record LogoutCommand : IRequest<Result>
    {
        public string? RefreshTokenRaw { get; init; }

        public LogoutCommand(string? refreshTokenRaw)
        {
            RefreshTokenRaw = refreshTokenRaw;
        }
    }
}