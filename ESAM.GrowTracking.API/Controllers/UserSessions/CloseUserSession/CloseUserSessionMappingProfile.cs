using AutoMapper;
using ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSession;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSession
{
    public class CloseUserSessionMappingProfile : Profile
    {
        public CloseUserSessionMappingProfile()
        {
            CreateMap<CloseUserSessionRequest, CloseUserSessionCommand>();
        }
    }
}