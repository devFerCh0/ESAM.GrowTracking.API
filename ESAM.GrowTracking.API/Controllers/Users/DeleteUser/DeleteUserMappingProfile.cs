using AutoMapper;
using ESAM.GrowTracking.Application.Features.Users.DeleteUser;

namespace ESAM.GrowTracking.API.Controllers.Users.DeleteUser
{
    public class DeleteUserMappingProfile : Profile
    {
        public DeleteUserMappingProfile()
        {
            CreateMap<DeleteUserRequest, DeleteUserCommand>();
        }
    }
}