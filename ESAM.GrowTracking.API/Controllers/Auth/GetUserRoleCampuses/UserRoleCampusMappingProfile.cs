using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses
{
    public class UserRoleCampusMappingProfile : Profile
    {
        public UserRoleCampusMappingProfile()
        {
            CreateMap<UserRoleCampusResponse, UserRoleCampusHttpResponse>();
        }
    }
}