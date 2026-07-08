using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.LogoutAllCurrent;

namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAllCurrent
{
    public class LogoutAllCurrentMappingProfile : Profile
    {
        public LogoutAllCurrentMappingProfile()
        {
            CreateMap<LogoutAllCurrentResponse, LogoutAllCurrentHttpResponse>();
        }
    }
}