using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions
{
    public class GetUserSessionsQueryValidator : AbstractValidator<GetUserSessionsQuery>
    {
        public GetUserSessionsQueryValidator()
        {
            RuleFor(gusq => gusq.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
            RuleFor(gusq => gusq.ApiClientType).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.ApiClientTypeInvalid)
                .When(gusq => gusq.ApiClientType.HasValue);
            RuleFor(gusq => gusq.SearchTerm).Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage(CommandValidationMessages.SearchTermMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.SearchTermMaxLength)
                .Must(CommandValidationRules.HasNoControlChars).WithMessage(CommandValidationMessages.SearchTermInvalid)
                .When(gusq => !string.IsNullOrWhiteSpace(gusq.SearchTerm));
            RuleFor(gusq => gusq.UserSessionsSortBy).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortByInvalid);
            RuleFor(gusq => gusq.SortDirection).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortDirectionInvalid);
            RuleFor(gusq => gusq.PageNumber).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage(CommandValidationMessages.PageNumberInvalid);
            RuleFor(gusq => gusq.PageSize).Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 100).WithMessage(CommandValidationMessages.PageSizeInvalid);
        }
    }
}