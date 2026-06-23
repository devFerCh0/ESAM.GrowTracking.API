using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusRequestValidator : AbstractValidator<AssumeRoleCampusRequest>
    {
        public AssumeRoleCampusRequestValidator()
        {
            RuleFor(arcc => arcc.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.WorkProfileIdRequired);
            RuleFor(arcc => arcc.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.RoleIdRequired);
            RuleFor(arcc => arcc.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CampusIdRequired);
        }
    }
}