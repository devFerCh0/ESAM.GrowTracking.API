using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus
{
    public class GetCurrentUserRoleCampusMappingProfile : Profile
    {
        public GetCurrentUserRoleCampusMappingProfile()
        {
            CreateMap<GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse, GetCurrentUserRoleCampusSessionRoleCampusSelectedHttpResponse>();
            CreateMap<GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse, GetCurrentUserRoleCampusSessionWorkProfileSelectedHttpResponse>();
            CreateMap<GetCurrentUserRoleCampusUserSessionResponse, GetCurrentUserRoleCampusUserSessionHttpResponse>();
            CreateMap<GetCurrentUserRoleCampusUserRoleCampusResponse, GetCurrentUserRoleCampusUserRoleCampusHttpResponse>();
            CreateMap<GetCurrentUserRoleCampusUserWorkProfileResponse, GetCurrentUserRoleCampusUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<GetCurrentUserRoleCampusResponse, GetCurrentUserRoleCampusHttpResponse>();
        }
    }
}