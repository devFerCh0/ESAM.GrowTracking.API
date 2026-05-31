using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public class LoginMappingProfile : Profile
    {
        public LoginMappingProfile()
        {
            CreateMap<LoginUserWorkProfileResponse, LoginUserWorkProfileHttpResponse>();
            CreateMap<LoginUserResponse, LoginUserHttpResponse>();
            CreateMap<LoginResponse, LoginHttpResponse>();
        }
    }
}