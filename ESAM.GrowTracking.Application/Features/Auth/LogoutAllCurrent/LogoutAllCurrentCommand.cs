using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.LogoutAllCurrent
{
    public record LogoutAllCurrentCommand : IRequest<Result<LogoutAllCurrentResponse>>
    {
        public LogoutAllCurrentCommand() { }
    }
}