using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileMappingProfile : Profile
    {
        public AssumeWorkProfileMappingProfile()
        {
            CreateMap<AssumeWorkProfileRequest, AssumeWorkProfileCommand>();
            CreateMap<AssumeWorkProfileSessionWorkProfileSelectedResponse, AssumeWorkProfileSessionWorkProfileSelectedHttpResponse>();
            CreateMap<AssumeWorkProfileUserSessionResponse, AssumeWorkProfileUserSessionHttpResponse>();
            CreateMap<AssumeWorkProfileUserWorkProfileResponse, AssumeWorkProfileUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<AssumeWorkProfileUserResponse, AssumeWorkProfileUserHttpResponse>();
            CreateMap<AssumeWorkProfileResponse, AssumeWorkProfileHttpResponse>();
        }
    }
}