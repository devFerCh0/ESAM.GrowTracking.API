using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus
{
    public class ChangeWorkProfileRoleCampusCommandValidator : AbstractValidator<ChangeWorkProfileRoleCampusCommand>
    {
        public ChangeWorkProfileRoleCampusCommandValidator()
        {
            RuleFor(cwprcc => cwprcc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
            RuleFor(cwprcc => cwprcc.RoleId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.RoleIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.RoleIdInvalid);
            RuleFor(cwprcc => cwprcc.CampusId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.CampusIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.CampusIdInvalid);
        }
    }
}