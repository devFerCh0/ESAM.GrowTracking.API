using ESAM.GrowTracking.Application.Validations;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices
{
    public class GetUserDevicesQueryValidator : AbstractValidator<GetUserDevicesQuery>
    {
        public GetUserDevicesQueryValidator()
        {
            RuleFor(gudq => gudq.UserId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(CommandValidationMessages.UserIdRequired)
                .GreaterThan(0).WithMessage(CommandValidationMessages.UserIdInvalid);
            RuleFor(gudq => gudq.ApiClientType).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.ApiClientTypeInvalid)
                .When(gudq => gudq.ApiClientType.HasValue);
            RuleFor(gudq => gudq.SearchTerm).Cascade(CascadeMode.Stop)
                .MinimumLength(2).WithMessage(CommandValidationMessages.SearchTermMinLength)
                .MaximumLength(100).WithMessage(CommandValidationMessages.SearchTermMaxLength)
                .Must(CommandValidationRules.HasNoControlChars).WithMessage(CommandValidationMessages.SearchTermInvalid)
                .When(gudq => !string.IsNullOrWhiteSpace(gudq.SearchTerm));
            RuleFor(gudq => gudq.UserDevicesSortBy).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortByInvalid);
            RuleFor(gusq => gusq.SortDirection).Cascade(CascadeMode.Stop)
                .IsInEnum().WithMessage(CommandValidationMessages.SortDirectionInvalid);
            RuleFor(gudq => gudq.PageNumber).Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage(CommandValidationMessages.PageNumberInvalid);
            RuleFor(gudq => gudq.PageSize).Cascade(CascadeMode.Stop)
                .InclusiveBetween(1, 100).WithMessage(CommandValidationMessages.PageSizeInvalid);
        }
    }
}