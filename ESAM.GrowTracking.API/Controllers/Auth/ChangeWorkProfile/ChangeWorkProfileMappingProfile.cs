using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfile
{
    public class ChangeWorkProfileMappingProfile : Profile
    {
        public ChangeWorkProfileMappingProfile()
        {
            CreateMap<ChangeWorkProfileRequest, ChangeWorkProfileCommand>();
            CreateMap<ChangeWorkProfileSessionWorkProfileSelectedResponse, ChangeWorkProfileSessionWorkProfileSelectedHttpResponse>();
            CreateMap<ChangeWorkProfileUserSessionResponse, ChangeWorkProfileUserSessionHttpResponse>();
            CreateMap<ChangeWorkProfileUserWorkProfileResponse, ChangeWorkProfileUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<ChangeWorkProfileUserResponse, ChangeWorkProfileUserHttpResponse>();
            CreateMap<ChangeWorkProfileResponse, ChangeWorkProfileHttpResponse>();
        }
    }
}