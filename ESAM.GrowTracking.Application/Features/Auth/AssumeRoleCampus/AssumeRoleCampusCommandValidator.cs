using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusCommandValidator : AbstractValidator<AssumeRoleCampusCommand>
    {
        public AssumeRoleCampusCommandValidator()
        {
            RuleFor(arcc => arcc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(ValidationMessages.WorkProfileIdInvalid);
            RuleFor(arcc => arcc.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.RoleIdRequired)
                .GreaterThan(0).WithMessage(ValidationMessages.RoleIdInvalid);
            RuleFor(arcc => arcc.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.CampusIdRequired)
                .GreaterThan(0).WithMessage(ValidationMessages.CampusIdInvalid);
        }
    }
}