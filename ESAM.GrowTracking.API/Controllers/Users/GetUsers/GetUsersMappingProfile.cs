using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.Users.GetUsers;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers
{
    public class GetUsersMappingProfile : Profile
    {
        public GetUsersMappingProfile()
        {
            CreateMap<GetUsersRequest, GetUsersQuery>()
                .ForCtorParam("usersSortBy", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.UsersSortBy) 
                    ? GetUsersSortBy.Username : EnumHelper.ParseFromString<GetUsersSortBy>(src.UsersSortBy)))
                .ForCtorParam("sortDirection", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SortDirection)
                    ? SortDirection.Ascending : EnumHelper.ParseFromString<SortDirection>(src.SortDirection)))
                .ForCtorParam("pageNumber", opt => opt.MapFrom(src => src.PageNumber ?? 1))
                .ForCtorParam("pageSize", opt => opt.MapFrom(src => src.PageSize ?? 20));
            CreateMap<GetUsersResponse.UserResponse.UserWorkProfileResponse, GetUsersHttpResponse.UserHttpResponse.UserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<GetUsersResponse.UserResponse, GetUsersHttpResponse.UserHttpResponse>();
            CreateMap<GetUsersResponse, GetUsersHttpResponse>();
            CreateMap(typeof(PagedResponse<>), typeof(PagedHttpResponse<>));
        }
    }
}