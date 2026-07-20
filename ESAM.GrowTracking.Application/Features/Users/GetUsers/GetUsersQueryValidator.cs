using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Users.GetUsers
{
    public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
    {
        public GetUsersQueryValidator()
        {
            RuleFor(guq => guq.SearchTerm).Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage(CommandValidationMessages.SearchTermMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.SearchTermMaxLength)
                .Must(CommandValidationRules.HasNoControlChars).WithMessage(CommandValidationMessages.SearchTermInvalid)
                .When(guq => !string.IsNullOrWhiteSpace(guq.SearchTerm));
            RuleFor(guq => guq.WorkProfileId).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage(CommandValidationMessages.WorkProfileIdInvalid)
                .When(guq => guq.WorkProfileId.HasValue);
            RuleFor(guq => guq.SortBy).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortByInvalid);
            RuleFor(guq => guq.SortDirection).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortDirectionInvalid);
            RuleFor(guq => guq.PageNumber).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage(CommandValidationMessages.PageNumberInvalid);
            RuleFor(guq => guq.PageSize).Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 100).WithMessage(CommandValidationMessages.PageSizeInvalid);
        }
    }
}