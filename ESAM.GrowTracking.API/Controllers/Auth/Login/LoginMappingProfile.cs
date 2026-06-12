using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses;
using ESAM.GrowTracking.API.Serialization;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public class LoginMappingProfile : Profile
    {
        public LoginMappingProfile()
        {
            CreateMap<LoginRequest, LoginCommand>()
                .ForCtorParam("apiClientType", opt => opt.MapFrom(src => EnumHelper.ParseFromString<ApiClientType>(src.ApiClientType)));
            CreateMap<LoginUserWorkProfileResponse, LoginUserWorkProfileHttpResponse>()
                .ForCtorParam("workProfileType", opt => opt.MapFrom(src => src.WorkProfileType.GetStringValue()));
            CreateMap<LoginUserResponse, LoginUserHttpResponse>();
            CreateMap<LoginResponse, LoginHttpResponse>();
        }
    }
}