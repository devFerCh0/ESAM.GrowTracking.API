using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.UserDevices.GetUserDevices
{
    public class GetUserDevicesMappingProfile : Profile
    {
        public GetUserDevicesMappingProfile()
        {
            CreateMap<GetUserDevicesRequest, GetUserDevicesQuery>()
                .ForCtorParam("apiClientType", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ApiClientType)
                    ? (ApiClientType?)null : EnumHelper.ParseFromString<ApiClientType>(src.ApiClientType)))
                .ForCtorParam("userDevicesSortBy", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.UserDevicesSortBy)
                    ? GetUserDevicesSortBy.CreatedAt : EnumHelper.ParseFromString<GetUserDevicesSortBy>(src.UserDevicesSortBy)))
                .ForCtorParam("sortDirection", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SortDirection)
                    ? SortDirection.Descending : EnumHelper.ParseFromString<SortDirection>(src.SortDirection)))
                .ForCtorParam("pageNumber", opt => opt.MapFrom(src => src.PageNumber ?? 1))
                .ForCtorParam("pageSize", opt => opt.MapFrom(src => src.PageSize ?? 20));
            CreateMap<GetUserDevicesResponse.UserDeviceResponse, GetUserDevicesHttpResponse.UserDeviceHttpResponse>()
                .ForCtorParam("apiClientType", opt => opt.MapFrom(src => src.ApiClientType.GetStringValue()));
            CreateMap<GetUserDevicesResponse, GetUserDevicesHttpResponse>();
            CreateMap(typeof(PagedResponse<>), typeof(PagedHttpResponse<>));
        }
    }
}