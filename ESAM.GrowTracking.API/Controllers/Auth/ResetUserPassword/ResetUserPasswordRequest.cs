namespace ESAM.GrowTracking.API.Controllers.Auth.ResetUserPassword
{
    public record ResetUserPasswordRequest
    {
        public int? UserId { get; init; }

        public string? NewPassword { get; init; }

        public string? ConfirmNewPassword { get; init; }

        public ResetUserPasswordRequest(int? userId, string? newPassword, string? confirmNewPassword)
        {
            UserId = userId;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}