namespace ESAM.GrowTracking.API.Controllers.Auth.ChangePassword
{
    public record ChangePasswordRequest
    {
        public string? CurrentPassword { get; init; }

        public string? NewPassword { get; init; }

        public string? ConfirmNewPassword { get; init; }

        public ChangePasswordRequest(string? currentPassword, string? newPassword, string? confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}