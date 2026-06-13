using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses
{
    public class GetUserRoleCampusesQueryValidator : AbstractValidator<GetUserRoleCampusesQuery>
    {
        public GetUserRoleCampusesQueryValidator()
        {
            RuleFor(gurcq => gurcq.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid);
        }
    }
}