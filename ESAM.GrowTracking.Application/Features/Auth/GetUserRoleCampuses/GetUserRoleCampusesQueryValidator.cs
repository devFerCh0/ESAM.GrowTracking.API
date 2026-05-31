using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses
{
    public class GetUserRoleCampusesQueryValidator : AbstractValidator<GetUserRoleCampusesQuery>
    {
        public GetUserRoleCampusesQueryValidator()
        {
            RuleFor(gurcq => gurcq.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(ValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(ValidationMessages.WorkProfileIdInvalid);
        }
    }
}