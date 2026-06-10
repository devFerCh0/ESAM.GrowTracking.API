using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses
{
    public class GetUserRoleCampusMappingProfile : Profile
    {
        public GetUserRoleCampusMappingProfile()
        {
            CreateMap<GetUserRoleCampusResponse, GetUserRoleCampusHttpResponse>();
        }
    }
}