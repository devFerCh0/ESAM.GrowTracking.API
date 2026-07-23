using ESAM.GrowTracking.API.Validations;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Utilities;
using FluentValidation;

namespace ESAM.GrowTracking.API.Controllers.UserDevices.GetUserDevices
{
    public sealed class GetUserDevicesRequestValidator : AbstractValidator<GetUserDevicesRequest>
    {
        public GetUserDevicesRequestValidator()
        {
            RuleFor(gudr => gudr.UserId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(RequestValidationMessages.UserIdRequired);
            RuleFor(gudr => gudr.ApiClientType).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<ApiClientType>).WithMessage(RequestValidationMessages.ApiClientTypeInvalid)
                .When(gudr => !string.IsNullOrWhiteSpace(gudr.ApiClientType));
            RuleFor(gudr => gudr.UserDevicesSortBy).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<GetUserDevicesSortBy>).WithMessage(RequestValidationMessages.SortByInvalid)
                .When(gudr => !string.IsNullOrWhiteSpace(gudr.UserDevicesSortBy));
            RuleFor(gudr => gudr.SortDirection).Cascade(CascadeMode.Stop)
                .Must(EnumHelper.IsValidFromString<SortDirection>).WithMessage(RequestValidationMessages.SortDirectionInvalid)
                .When(gudr => !string.IsNullOrWhiteSpace(gudr.SortDirection));
        }
    }
}