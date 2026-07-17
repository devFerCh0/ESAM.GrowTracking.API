using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.ResetUserPassword
{
    public record ResetUserPasswordCommand : IRequest<Result>
    {
        public int UserId { get; init; }

        public string NewPassword { get; init; }

        public string ConfirmNewPassword { get; init; }

        public ResetUserPasswordCommand(int userId, string newPassword, string confirmNewPassword)
        {
            UserId = userId;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}