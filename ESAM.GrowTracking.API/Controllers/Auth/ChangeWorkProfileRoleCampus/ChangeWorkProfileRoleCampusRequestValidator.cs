using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus
{
    public class ChangeWorkProfileRoleCampusRequestValidator : AbstractValidator<ChangeWorkProfileRoleCampusRequest>
    {
        public ChangeWorkProfileRoleCampusRequestValidator()
        {
            RuleFor(cwprcr => cwprcr.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.WorkProfileIdRequired);
            RuleFor(cwprcr => cwprcr.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.RoleIdRequired);
            RuleFor(cwprcr => cwprcr.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CampusIdRequired);
        }
    }
}