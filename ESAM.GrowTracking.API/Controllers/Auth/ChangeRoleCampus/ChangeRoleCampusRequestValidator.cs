using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus
{
    public class ChangeRoleCampusRequestValidator : AbstractValidator<ChangeRoleCampusRequest>
    {
        public ChangeRoleCampusRequestValidator()
        {
            RuleFor(arcc => arcc.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.RoleIdRequired);
            RuleFor(arcc => arcc.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CampusIdRequired);
        }
    }
}