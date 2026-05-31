using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile
{
    public class AssumeWorkProfileMappingProfile : Profile
    {
        public AssumeWorkProfileMappingProfile()
        {
            CreateMap<AssumeWorkProfileSessionWorkProfileSelectedResponse, AssumeWorkProfileSessionWorkProfileSelectedHttpResponse>();
            CreateMap<AssumeWorkProfileUserSessionResponse, AssumeWorkProfileUserSessionHttpResponse>();
            CreateMap<AssumeWorkProfileUserWorkProfileResponse, AssumeWorkProfileUserWorkProfileHttpResponse>();
            CreateMap<AssumeWorkProfileUserResponse, AssumeWorkProfileUserHttpResponse>();
            CreateMap<AssumeWorkProfileResponse, AssumeWorkProfileHttpResponse>();
        }
    }
}