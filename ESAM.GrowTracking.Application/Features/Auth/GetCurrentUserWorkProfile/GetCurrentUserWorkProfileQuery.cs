using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile
{
    public record GetCurrentUserWorkProfileQuery :IRequest<Result<GetCurrentUserWorkProfileResponse>>
    {
        public GetCurrentUserWorkProfileQuery() { }
    }
}