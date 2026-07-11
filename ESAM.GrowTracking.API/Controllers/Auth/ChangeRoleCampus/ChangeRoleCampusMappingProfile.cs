using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeRoleCampus
{
    public class ChangeRoleCampusMappingProfile : Profile
    {
        public ChangeRoleCampusMappingProfile()
        {
            CreateMap<ChangeRoleCampusRequest, ChangeRoleCampusCommand>();
            CreateMap<ChangeRoleCampusSessionRoleCampusSelectedResponse, ChangeRoleCampusSessionRoleCampusSelectedHttpResponse>();
            CreateMap<ChangeRoleCampusSessionWorkProfileSelectedResponse, ChangeRoleCampusSessionWorkProfileSelectedHttpResponse>();
            CreateMap<ChangeRoleCampusUserSessionResponse, ChangeRoleCampusUserSessionHttpResponse>();
            CreateMap<ChangeRoleCampusUserRoleCampusResponse, ChangeRoleCampusUserRoleCampusHttpResponse>();
            CreateMap<ChangeRoleCampusUserWorkProfileResponse, ChangeRoleCampusUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<ChangeRoleCampusUserResponse, ChangeRoleCampusUserHttpResponse>();
            CreateMap<ChangeRoleCampusResponse, ChangeRoleCampusHttpResponse>();
        }
    }
}