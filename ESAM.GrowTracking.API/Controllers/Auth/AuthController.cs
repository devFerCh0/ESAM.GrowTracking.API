using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.API.Controllers.Auth.Login;
using ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.Logout;
using ESAM.GrowTracking.API.Controllers.Auth.Refresh;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Logout;
using ESAM.GrowTracking.Application.Features.Auth.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ESAM.GrowTracking.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;
        private readonly IAuthSessionCookieService _authSessionCookieService;

        public AuthController(IMapper mapper, ISender sender, IErrorToHttpMapper errorToHttpMapper, IAuthSessionCookieService authSessionCookieService)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            ArgumentNullException.ThrowIfNull(authSessionCookieService);
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
            _authSessionCookieService = authSessionCookieService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<LoginHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<LoginHttpResponse>>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(request.Credential, request.Password, request.IsPersistent, request.DeviceIdentifier, request.DeviceName, request.ApiClientType);
            var loginResult = await _sender.Send(command, cancellationToken);
            if (loginResult.IsFailure)
                return loginResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var login = _mapper.Map<LoginHttpResponse>(loginResult.Value);
            return Ok(ApiSuccessResponse<LoginHttpResponse>.From(login, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpPost("assume-work-profile")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<AssumeWorkProfileHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<AssumeWorkProfileHttpResponse>>> AssumeWorkProfileAsync([FromBody] AssumeWorkProfileRequest request, 
            CancellationToken cancellationToken)
        {
            var command = new AssumeWorkProfileCommand(request.WorkProfileId);
            var assumeWorkProfileResult = await _sender.Send(command, cancellationToken);
            if (assumeWorkProfileResult.IsFailure)
                return assumeWorkProfileResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var assumeWorkProfile = _mapper.Map<AssumeWorkProfileHttpResponse>(assumeWorkProfileResult.Value);
            _authSessionCookieService.SetSessionCookies(assumeWorkProfile.RefreshTokenRaw, assumeWorkProfile.RefreshTokenExpiresAt);
            return Ok(ApiSuccessResponse<AssumeWorkProfileHttpResponse>.From(assumeWorkProfile, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpGet("work-profile/{workProfileId:int}/user-role-campuses")]
        [ProducesResponseType(typeof(ApiSuccessResponse<List<UserRoleCampusHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiSuccessResponse<List<UserRoleCampusHttpResponse>>>> GetUserRoleCampusesAsync([FromRoute] int workProfileId, 
            CancellationToken cancellationToken)
        {
            var query = new GetUserRoleCampusesQuery(workProfileId);
            var userRoleCampusesResult = await _sender.Send(query, cancellationToken);
            if (userRoleCampusesResult.IsFailure)
                return userRoleCampusesResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var userRoleCampuses = _mapper.Map<List<UserRoleCampusHttpResponse>>(userRoleCampusesResult.Value);
            return Ok(ApiSuccessResponse<List<UserRoleCampusHttpResponse>>.From(userRoleCampuses, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpPost("assume-role-campus")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<AssumeRoleCampusHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<AssumeRoleCampusHttpResponse>>> AssumeRoleCampusAsync([FromBody] AssumeRoleCampusRequest request, 
            CancellationToken cancellationToken)
        {
            var command = new AssumeRoleCampusCommand(request.WorkProfileId, request.RoleId, request.CampusId);
            var assumeRoleCampusResult = await _sender.Send(command, cancellationToken);
            if (assumeRoleCampusResult.IsFailure)
                return assumeRoleCampusResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var assumeRoleCampus = _mapper.Map<AssumeRoleCampusHttpResponse>(assumeRoleCampusResult.Value);
            _authSessionCookieService.SetSessionCookies(assumeRoleCampus.RefreshTokenRaw, assumeRoleCampus.RefreshTokenExpiresAt);
            return Ok(ApiSuccessResponse<AssumeRoleCampusHttpResponse>.From(assumeRoleCampus, HttpContext.TraceIdentifier));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<RefreshHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiSuccessResponse<RefreshHttpResponse>>> RefreshAsync([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshRequest? request,
            CancellationToken cancellationToken)
        {
            var resolvedRefreshToken = _authSessionCookieService.ResolveRefreshToken(request?.RefreshTokenRaw);
            var command = new RefreshCommand(resolvedRefreshToken, request?.DeviceIdentifier);
            var refreshResult = await _sender.Send(command, cancellationToken);
            if (refreshResult.IsFailure)
            {
                if (_authSessionCookieService.RequiresCookieClearOnFailure(refreshResult.Errors))
                    _authSessionCookieService.ClearSessionCookies();
                return refreshResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            }
            var refresh = _mapper.Map<RefreshHttpResponse>(refreshResult.Value);
            _authSessionCookieService.SetSessionCookies(refresh.RefreshTokenRaw, refresh.RefreshTokenExpiresAt);
            return Ok(ApiSuccessResponse<RefreshHttpResponse>.From(refresh, HttpContext.TraceIdentifier));
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiSuccessResponse>> LogoutAsync([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LogoutRequest? request, 
            CancellationToken cancellationToken)
        {
            var resolvedRefreshToken = _authSessionCookieService.ResolveRefreshToken(request?.RefreshTokenRaw);
            var command = new LogoutCommand(resolvedRefreshToken, request?.DeviceIdentifier);
            try
            {
                var logoutResult = await _sender.Send(command, cancellationToken);
                if (logoutResult.IsFailure)
                    return logoutResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
                return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
            }
            finally
            {
                _authSessionCookieService.ClearSessionCookies();
            }
        }
    }
}