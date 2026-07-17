using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.ChangePassword;

namespace ESAM.GrowTracking.API.Controllers.Auth.ChangePassword
{
    public class ChangePasswordMappingProfile : Profile
    {
        public ChangePasswordMappingProfile()
        {
            CreateMap<ChangePasswordRequest, ChangePasswordCommand>();
            CreateMap<ChangePasswordResponse, ChangePasswordHttpResponse>();
        }
    }
}