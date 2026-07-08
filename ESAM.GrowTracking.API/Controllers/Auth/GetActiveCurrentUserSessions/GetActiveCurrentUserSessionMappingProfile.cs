using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.GetActiveCurrentUserSessions.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetActiveCurrentUserSessions
{
    public class GetActiveCurrentUserSessionMappingProfile : Profile
    {
        public GetActiveCurrentUserSessionMappingProfile()
        {
            CreateMap<GetActiveCurrentUserSessionRoleCampusResponse, GetActiveCurrentUserSessionRoleCampusHttpResponse>();
            CreateMap<GetActiveCurrentUserSessionWorkProfileResponse, GetActiveCurrentUserSessionWorkProfileHttpResponse>();
            CreateMap<GetActiveCurrentUserSessionsResponse, GetActiveCurrentUserSessionsHttpResponse>();
        }
    }
}