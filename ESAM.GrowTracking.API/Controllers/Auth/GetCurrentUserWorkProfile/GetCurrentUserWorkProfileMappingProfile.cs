using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile
{
    public class GetCurrentUserWorkProfileMappingProfile : Profile
    {
        public GetCurrentUserWorkProfileMappingProfile()
        {
            CreateMap<GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse, GetCurrentUserWorkProfileSessionWorkProfileSelectedHttpResponse>();
            CreateMap<GetCurrentUserWorkProfileUserSessionResponse, GetCurrentUserWorkProfileUserSessionHttpResponse>();
            CreateMap<GetCurrentUserWorkProfileUserWorkProfileResponse, GetCurrentUserWorkProfileUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.ToString()));
            CreateMap<GetCurrentUserWorkProfileResponse, GetCurrentUserWorkProfileHttpResponse>();
        }
    }
}