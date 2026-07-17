using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.UnlockUserAccount;

namespace ESAM.GrowTracking.API.Controllers.Auth.UnlockUserDevice
{
    public class UnlockUserDeviceMappingProfile : Profile
    {
        public UnlockUserDeviceMappingProfile()
        {
            CreateMap<UnlockUserDeviceRequest, UnlockUserDeviceCommand>();
        }
    }
}