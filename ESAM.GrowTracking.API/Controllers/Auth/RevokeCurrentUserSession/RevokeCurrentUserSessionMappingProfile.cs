using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.RevokeCurrentUserSession;

namespace ESAM.GrowTracking.API.Controllers.Auth.RevokeCurrentUserSession
{
    public class RevokeCurrentUserSessionMappingProfile : Profile
    {
        public RevokeCurrentUserSessionMappingProfile()
        {
            CreateMap<RevokeCurrentUserSessionRequest, RevokeCurrentUserSessionCommand>();
        }
    }
}