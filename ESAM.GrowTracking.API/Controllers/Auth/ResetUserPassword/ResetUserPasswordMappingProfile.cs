using AutoMapper;
using ESAM.GrowTracking.Application.Features.Users.ResetUserPassword;

namespace ESAM.GrowTracking.API.Controllers.Auth.ResetUserPassword
{
    public class ResetUserPasswordMappingProfile : Profile
    {
        public ResetUserPasswordMappingProfile()
        {
            CreateMap<ResetUserPasswordRequest, ResetUserPasswordCommand>();
        }
    }
}