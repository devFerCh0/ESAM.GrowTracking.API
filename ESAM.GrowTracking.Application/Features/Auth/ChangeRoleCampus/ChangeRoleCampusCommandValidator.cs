using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus
{
    public class ChangeRoleCampusCommandValidator : AbstractValidator<ChangeRoleCampusCommand>
    {
        public ChangeRoleCampusCommandValidator()
        {
            RuleFor(crcc => crcc.RoleId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.RoleIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.RoleIdInvalid);
            RuleFor(crcc => crcc.CampusId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.CampusIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.CampusIdInvalid);
        }
    }
}