using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.LogoutAll;

namespace ESAM.GrowTracking.API.Controllers.Auth.LogoutAll
{
    public class LogoutAllMappingProfile : Profile
    {
        public LogoutAllMappingProfile()
        {
            CreateMap<LogoutAllRequest, LogoutAllCommand>();
            CreateMap<LogoutAllResponse, LogoutAllHttpResponse>();
        }
    }
}