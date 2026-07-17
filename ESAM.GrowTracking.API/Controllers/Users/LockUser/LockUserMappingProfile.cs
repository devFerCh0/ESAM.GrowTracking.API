using AutoMapper;
using ESAM.GrowTracking.Application.Features.Users.LockUser;

namespace ESAM.GrowTracking.API.Controllers.Users.LockUser
{
    public class LockUserMappingProfile : Profile
    {
        public LockUserMappingProfile()
        {
            CreateMap<LockUserRequest, LockUserCommand>();
        }
    }
}