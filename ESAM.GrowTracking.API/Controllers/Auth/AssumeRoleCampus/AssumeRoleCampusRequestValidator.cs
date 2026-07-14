using ESAM.GrowTracking.API.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusRequestValidator : AbstractValidator<AssumeRoleCampusRequest>
    {
        public AssumeRoleCampusRequestValidator()
        {
            RuleFor(arcr => arcr.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.WorkProfileIdRequired);
            RuleFor(arcr => arcr.RoleId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.RoleIdRequired);
            RuleFor(arcr => arcr.CampusId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.CampusIdRequired);
        }
    }
}