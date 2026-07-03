using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(rc => rc.AccessToken).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.AccessTokenRequired)
                .MinimumLength(256).WithMessage(CommandValidationMessages.AccessTokenMinLength)
                .MaximumLength(1024).WithMessage(CommandValidationMessages.AccessTokenMaxLength);
            RuleFor(rc => rc.RefreshTokenRaw).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.RefreshTokenRequired)
                .MinimumLength(32).WithMessage(CommandValidationMessages.RefreshTokenMinLength)
                .MaximumLength(256).WithMessage(CommandValidationMessages.RefreshTokenMaxLength)
                .Must(rt =>
                {
                    var p = rt!.Split('.');
                    return p.Length == 2 && p[0].Length > 0 && p[1].Length > 0;
                }).WithMessage(CommandValidationMessages.RefreshTokenInvalid);
        }
    }
}