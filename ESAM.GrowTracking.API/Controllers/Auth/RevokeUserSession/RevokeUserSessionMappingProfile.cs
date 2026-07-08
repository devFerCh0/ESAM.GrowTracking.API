using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.RevokeUserSession;

namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeUserSession
{
    public class RevokeUserSessionMappingProfile : Profile
    {
        public RevokeUserSessionMappingProfile()
        {
            CreateMap<RevokeUserSessionRequest, RevokeUserSessionCommand>();
        }
    }
}