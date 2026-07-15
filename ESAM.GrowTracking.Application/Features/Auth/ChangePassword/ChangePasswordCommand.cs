using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangePassword
{
    public record ChangePasswordCommand : IRequest<Result<ChangePasswordResponse>>
    {
        public string CurrentPassword { get; init; }

        public string NewPassword { get; init; }

        public string ConfirmNewPassword { get; init; }

        public ChangePasswordCommand(string currentPassword, string newPassword, string confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}