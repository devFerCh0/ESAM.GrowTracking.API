using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus
{
    public class ChangeRoleCampusRequestValidator : AbstractValidator<ChangeRoleCampusRequest>
    {
        public ChangeRoleCampusRequestValidator()
        {
            RuleFor(crcr => crcr.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.RoleIdRequired);
            RuleFor(crcr => crcr.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CampusIdRequired);
        }
    }
}