using AutoMapper;
using ESAM.GrowTracking.Application.Features.Users.RestoreUser;

namespace ESAM.GrowTracking.API.Controllers.Users.RestoreUser
{
    public class RestoreUserMappingProfile : Profile
    {
        public RestoreUserMappingProfile()
        {
            CreateMap<RestoreUserRequest, RestoreUserCommand>();
        }
    }
}