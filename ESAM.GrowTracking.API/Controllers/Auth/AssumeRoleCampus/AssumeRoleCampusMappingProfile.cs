using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusMappingProfile : Profile
    {
        public AssumeRoleCampusMappingProfile()
        {
            CreateMap<AssumeRoleCampusSessionRoleCampusSelectedResponse, AssumeRoleCampusSessionRoleCampusSelectedHttpResponse>();
            CreateMap<AssumeRoleCampusSessionWorkProfileSelectedResponse, AssumeRoleCampusSessionWorkProfileSelectedHttpResponse>();
            CreateMap<AssumeRoleCampusUserSessionResponse, AssumeRoleCampusUserSessionHttpResponse>();
            CreateMap<AssumeRoleCampusUserRoleCampusResponse, AssumeRoleCampusUserRoleCampusHttpResponse>();
            CreateMap<AssumeRoleCampusUserWorkProfileResponse, AssumeRoleCampusUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.ToString()));
            CreateMap<AssumeRoleCampusUserResponse, AssumeRoleCampusUserHttpResponse>();
            CreateMap<AssumeRoleCampusResponse, AssumeRoleCampusHttpResponse>();
        }
    }
}