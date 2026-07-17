using AutoMapper;
using ESAM.GrowTracking.Application.Features.Users.UnlockUser;

namespace ESAM.GrowTracking.API.Controllers.Users.UnlockUser
{
    public class UnlockUserMappingProfile : Profile
    {
        public UnlockUserMappingProfile()
        {

            CreateMap<UnlockUserRequest, UnlockUserCommand>();
        }
    }
}