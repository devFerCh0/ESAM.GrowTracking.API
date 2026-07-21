using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Users.GetUsers.HttpResponses;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.Users.GetUsers;
using ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses;
using ESAM.GrowTracking.Infrastructure.Utilities;

namespace ESAM.GrowTracking.API.Controllers.Users.GetUsers
{
    public class GetUsersMappingProfile : Profile
    {
        public GetUsersMappingProfile()
        {
            CreateMap<GetUsersRequest, GetUsersQuery>()
                .ForCtorParam("sortBy", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SortBy) 
                    ? GetUsersSortBy.Username : EnumHelper.ParseFromString<GetUsersSortBy>(src.SortBy)))
                .ForCtorParam("sortDirection", opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.SortDirection)
                    ? SortDirection.Ascending : EnumHelper.ParseFromString<SortDirection>(src.SortDirection)))
                .ForCtorParam("pageNumber", opt => opt.MapFrom(src => src.PageNumber ?? 1))
                .ForCtorParam("pageSize", opt => opt.MapFrom(src => src.PageSize ?? 20));
            CreateMap<GetUsersUserWorkProfileResponse, GetUsersUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<GetUsersResponse, GetUsersHttpResponse>();
            CreateMap<PagedResponse<GetUsersResponse>, PagedHttpResponse<GetUsersHttpResponse>>();
        }
    }
}