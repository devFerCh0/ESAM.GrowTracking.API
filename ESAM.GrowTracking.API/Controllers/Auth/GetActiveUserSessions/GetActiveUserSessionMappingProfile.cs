using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.GetActiveUserSessions.HttpResponses;
using ESAM.GrowTracking.Application.Features.Users.GetActiveUserSessions.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetActiveUserSessions
{
    public class GetActiveUserSessionMappingProfile : Profile
    {
        public GetActiveUserSessionMappingProfile()
        {
            CreateMap<GetActiveUserSessionRoleCampusResponse, GetActiveUserSessionRoleCampusHttpResponse>();
            CreateMap<GetActiveUserSessionWorkProfileResponse, GetActiveUserSessionWorkProfileHttpResponse>();
            CreateMap<GetActiveUserSessionsResponse, GetActiveUserSessionsHttpResponse>();
        }
    }
}