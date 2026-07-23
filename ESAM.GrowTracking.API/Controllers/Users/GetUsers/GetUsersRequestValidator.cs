using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.Users.GetUsers;
using ESAM.GrowTracking.Infrastructure.Utilities;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers
{
    public sealed class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
    {
        public GetUsersRequestValidator()
        {
            RuleFor(gur => gur.UsersSortBy).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<GetUsersSortBy>).WithMessage(RequestValidationMessages.SortByInvalid)
                .When(gur => !string.IsNullOrWhiteSpace(gur.UsersSortBy));
            RuleFor(gur => gur.SortDirection).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<SortDirection>).WithMessage(RequestValidationMessages.SortDirectionInvalid)
                .When(gur => !string.IsNullOrWhiteSpace(gur.SortDirection));
        }
    }
}