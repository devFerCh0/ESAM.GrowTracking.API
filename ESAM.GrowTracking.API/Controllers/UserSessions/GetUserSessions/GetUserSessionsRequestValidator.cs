using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Utilities;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.GetUserSessions
{
    public sealed class GetUserSessionsRequestValidator : AbstractValidator<GetUserSessionsRequest>
    {
        public GetUserSessionsRequestValidator()
        {
            RuleFor(gusr => gusr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(gusr => gusr.ApiClientType).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<ApiClientType>).WithMessage(RequestValidationMessages.ApiClientTypeInvalid)
                .When(gusr => !string.IsNullOrWhiteSpace(gusr.ApiClientType));
            RuleFor(gusr => gusr.UserSessionsSortBy).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<GetUserSessionsSortBy>).WithMessage(RequestValidationMessages.SortByInvalid)
                .When(gusr => !string.IsNullOrWhiteSpace(gusr.UserSessionsSortBy));
            RuleFor(gusr => gusr.SortDirection).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<SortDirection>).WithMessage(RequestValidationMessages.SortDirectionInvalid)
                .When(gusr => !string.IsNullOrWhiteSpace(gusr.SortDirection));
        }
    }
}