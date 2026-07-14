using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangeWorkProfileRoleCampus
{
    public class ChangeWorkProfileRoleCampusMappingProfile : Profile
    {
        public ChangeWorkProfileRoleCampusMappingProfile()
        {
            CreateMap<ChangeWorkProfileRoleCampusRequest, ChangeWorkProfileRoleCampusCommand>();
            CreateMap<ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse, ChangeWorkProfileRoleCampusSessionRoleCampusSelectedHttpResponse>();
            CreateMap<ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse, ChangeWorkProfileRoleCampusSessionWorkProfileSelectedHttpResponse>();
            CreateMap<ChangeWorkProfileRoleCampusUserSessionResponse, ChangeWorkProfileRoleCampusUserSessionHttpResponse>();
            CreateMap<ChangeWorkProfileRoleCampusUserRoleCampusResponse, ChangeWorkProfileRoleCampusUserRoleCampusHttpResponse>();
            CreateMap<ChangeWorkProfileRoleCampusUserWorkProfileResponse, ChangeWorkProfileRoleCampusUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<ChangeWorkProfileRoleCampusUserResponse, ChangeWorkProfileRoleCampusUserHttpResponse>();
            CreateMap<ChangeWorkProfileRoleCampusResponse, ChangeWorkProfileRoleCampusHttpResponse>();
        }
    }
}