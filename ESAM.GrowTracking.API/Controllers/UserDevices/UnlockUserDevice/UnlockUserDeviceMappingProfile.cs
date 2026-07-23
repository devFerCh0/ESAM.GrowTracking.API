using AutoMapper;
using ESAM.GrowTracking.Application.Features.UserDevices.UnlockUserDevice;

namespace ESAM.GrowTracking.API.Controllers.UserDevices.UnlockUserDevice
{
    public class UnlockUserDeviceMappingProfile : Profile
    {
        public UnlockUserDeviceMappingProfile()
        {
            CreateMap<UnlockUserDeviceRequest, UnlockUserDeviceCommand>();
        }
    }
}