using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices;
namespace ESAM.GrowTracking.API.Controllers.Auth.GetLockedUserDevices
{
    public class GetLockedUserDeviceMappingProfile : Profile
    {
        public GetLockedUserDeviceMappingProfile()
        {
            CreateMap<GetLockedUserDeviceResponse, GetLockedUserDeviceHttpResponse>();
            //CreateMap<GetLockedUserDeviceResponse, GetLockedUserDeviceHttpResponse>()
            //    .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.ApiClientType.GetStringValue()));
        }
    }
}