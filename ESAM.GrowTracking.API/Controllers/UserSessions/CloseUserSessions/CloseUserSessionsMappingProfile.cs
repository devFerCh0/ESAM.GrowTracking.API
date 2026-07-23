using AutoMapper;
using ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSessions;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSessions
{
    public class CloseUserSessionsMappingProfile : Profile
    {
        public CloseUserSessionsMappingProfile()
        {
            CreateMap<CloseUserSessionsRequest, CloseUserSessionsCommand>();
        }
    }
}