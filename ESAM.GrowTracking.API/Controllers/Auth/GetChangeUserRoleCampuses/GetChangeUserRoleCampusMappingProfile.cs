using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.GetChangeUserRoleCampuses;

namespace ESAM.GrowTracking.API.Controllers.Auth.GetChangeUserRoleCampuses
{
    public class GetChangeUserRoleCampusMappingProfile : Profile
    {
        public GetChangeUserRoleCampusMappingProfile()
        {
            CreateMap<GetChangeUserRoleCampusResponse, GetChangeUserRoleCampusHttpResponse>();
        }
    }
}