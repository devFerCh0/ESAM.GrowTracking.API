using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.GetChangeUserRoleCampuses
{
    public class GetChangeUserRoleCampusesQueryValidator : AbstractValidator<GetChangeUserRoleCampusesQuery>
    {
        public GetChangeUserRoleCampusesQueryValidator()
        {
            RuleFor(gcurcq => gcurcq.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
        }
    }
}