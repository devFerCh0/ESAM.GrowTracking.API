using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.Refresh;

namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public class RefreshMappingProfile : Profile
    {
        public RefreshMappingProfile()
        {
            CreateMap<RefreshRequest, RefreshCommand>();
            CreateMap<RefreshResponse, RefreshHttpResponse>();
        }
    }
}