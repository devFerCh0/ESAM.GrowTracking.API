using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.UserSessions.GetUserSessions
{
    public class GetUserSessionsMappingProfile : Profile
    {
        public GetUserSessionsMappingProfile()
        {
            CreateMap<GetUserSessionsRequest, GetUserSessionsQuery>()
                .ForCtorParam("apiClientType", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ApiClientType)
                    ? (ApiClientType?)null : EnumHelper.ParseFromString<ApiClientType>(src.ApiClientType)))
                .ForCtorParam("userSessionsSortBy", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.UserSessionsSortBy)
                    ? GetUserSessionsSortBy.CreatedAt : EnumHelper.ParseFromString<GetUserSessionsSortBy>(src.UserSessionsSortBy)))
                .ForCtorParam("sortDirection", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SortDirection)
                    ? SortDirection.Descending : EnumHelper.ParseFromString<SortDirection>(src.SortDirection)))
                .ForCtorParam("pageNumber", opt => opt.MapFrom(src => src.PageNumber ?? 1))
                .ForCtorParam("pageSize", opt => opt.MapFrom(src => src.PageSize ?? 20));
            CreateMap<GetUserSessionsResponse.UserSessionResponse.UserSessionWorkProfileSelectedResponse.UserSessionWorkProfileRoleCampusSelectedResponse,
                GetUserSessionsHttpResponse.UserSessionHttpResponse.UserSessionWorkProfileSelectedHttpResponse.UserSessionWorkProfileRoleCampusSelectedHttpResponse>();
            CreateMap<GetUserSessionsResponse.UserSessionResponse.UserSessionWorkProfileSelectedResponse, 
                GetUserSessionsHttpResponse.UserSessionHttpResponse.UserSessionWorkProfileSelectedHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<GetUserSessionsResponse.UserSessionResponse, GetUserSessionsHttpResponse.UserSessionHttpResponse>()
                .ForCtorParam("apiClientType", opt => opt.MapFrom(src => src.ApiClientType.GetStringValue()));
            CreateMap<GetUserSessionsResponse, GetUserSessionsHttpResponse>();
            CreateMap(typeof(PagedResponse<>), typeof(PagedHttpResponse<>));
        }
    }
}