using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.ResetUserPassword;

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