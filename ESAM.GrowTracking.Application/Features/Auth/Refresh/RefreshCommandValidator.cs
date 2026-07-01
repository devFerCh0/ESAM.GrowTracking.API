using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(rc => rc.RefreshTokenRaw).Cascade(CascadeMode.Stop)
                .MinimumLength(32).WithMessage(CommandValidationMessages.RefreshTokenMinLength)
                .MaximumLength(256).WithMessage(CommandValidationMessages.RefreshTokenMaxLength)
                .Must(rt =>
                {
                    var p = rt!.Split('.');
                    return p.Length == 2 && p[0].Length > 0 && p[1].Length > 0;
                }).WithMessage(CommandValidationMessages.RefreshTokenInvalid)
                .When(rc => !string.IsNullOrWhiteSpace(rc.RefreshTokenRaw));
        }
    }
}