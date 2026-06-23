using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusCommandValidator : AbstractValidator<AssumeRoleCampusCommand>
    {
        public AssumeRoleCampusCommandValidator()
        {
            RuleFor(arcc => arcc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
            RuleFor(arcc => arcc.RoleId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.RoleIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.RoleIdInvalid);
            RuleFor(arcc => arcc.CampusId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.CampusIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.CampusIdInvalid);
        }
    }
}