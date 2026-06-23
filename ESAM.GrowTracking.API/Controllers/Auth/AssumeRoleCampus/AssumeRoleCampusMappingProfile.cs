using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus
{
    public class AssumeRoleCampusMappingProfile : Profile
    {
        public AssumeRoleCampusMappingProfile()
        {
            CreateMap<AssumeRoleCampusRequest, AssumeRoleCampusCommand>();
            CreateMap<AssumeRoleCampusSessionRoleCampusSelectedResponse, AssumeRoleCampusSessionRoleCampusSelectedHttpResponse>();
            CreateMap<AssumeRoleCampusSessionWorkProfileSelectedResponse, AssumeRoleCampusSessionWorkProfileSelectedHttpResponse>();
            CreateMap<AssumeRoleCampusUserSessionResponse, AssumeRoleCampusUserSessionHttpResponse>();
            CreateMap<AssumeRoleCampusUserRoleCampusResponse, AssumeRoleCampusUserRoleCampusHttpResponse>();
            CreateMap<AssumeRoleCampusUserWorkProfileResponse, AssumeRoleCampusUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<AssumeRoleCampusUserResponse, AssumeRoleCampusUserHttpResponse>();
            CreateMap<AssumeRoleCampusResponse, AssumeRoleCampusHttpResponse>();
        }
    }
}