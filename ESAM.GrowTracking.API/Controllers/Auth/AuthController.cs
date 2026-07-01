using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeRoleCampus.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.API.Controllers.Auth.AssumeWorkProfile.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserRoleCampus.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.GetCurrentUserWorkProfile.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.API.Controllers.Auth.Login;
using ESAM.GrowTracking.API.Controllers.Auth.Login.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Auth.Logout;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application.Abstractions.Services;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.GetUserRoleCampuses;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Logout;
using FluentValidation;
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
        private readonly IValidator<LoginRequest> _loginRequesValidator;
        private readonly IValidator<AssumeRoleCampusRequest> _assumeRoleCampusRequestValidator;
        private readonly IValidator<AssumeWorkProfileRequest> _assumeWorkProfileRequestValidator;
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;
        private readonly IAuthSessionCookieService _authSessionCookieService;

        public AuthController(IValidator<LoginRequest> loginRequesValidator, IValidator<AssumeRoleCampusRequest> assumeRoleCampusRequestValidator, 
            IValidator<AssumeWorkProfileRequest> assumeWorkProfileRequestValidator, IMapper mapper, ISender sender, IErrorToHttpMapper errorToHttpMapper, 
            IAuthSessionCookieService authSessionCookieService)
        {
            ArgumentNullException.ThrowIfNull(loginRequesValidator);
            ArgumentNullException.ThrowIfNull(assumeRoleCampusRequestValidator);
            ArgumentNullException.ThrowIfNull(assumeWorkProfileRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            ArgumentNullException.ThrowIfNull(authSessionCookieService);
            _loginRequesValidator = loginRequesValidator;
            _assumeRoleCampusRequestValidator = assumeRoleCampusRequestValidator;
            _assumeWorkProfileRequestValidator = assumeWorkProfileRequestValidator;
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
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<LoginHttpResponse>>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var validation = await _loginRequesValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<LoginCommand>(request);
            var loginResult = await _sender.Send(command, cancellationToken);
            if (loginResult.IsFailure)
                return loginResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var login = _mapper.Map<LoginHttpResponse>(loginResult.Value);
            return Ok(ApiSuccessResponse<LoginHttpResponse>.From(login, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireTemporaryTypeAccessToken)]
        [HttpGet("work-profile/{workProfileId:int}/user-role-campuses")]
        [ProducesResponseType(typeof(ApiSuccessResponse<List<GetUserRoleCampusHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<List<GetUserRoleCampusHttpResponse>>>> GetUserRoleCampusesAsync([FromRoute] int workProfileId, 
            CancellationToken cancellationToken)
        {
            var query = new GetUserRoleCampusesQuery(workProfileId);
            var userRoleCampusesResult = await _sender.Send(query, cancellationToken);
            if (userRoleCampusesResult.IsFailure)
                return userRoleCampusesResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var userRoleCampuses = _mapper.Map<List<GetUserRoleCampusHttpResponse>>(userRoleCampusesResult.Value);
            return Ok(ApiSuccessResponse<List<GetUserRoleCampusHttpResponse>>.From(userRoleCampuses, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireTemporaryTypeAccessToken)]
        [HttpPost("assume-role-campus")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<AssumeRoleCampusHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<AssumeRoleCampusHttpResponse>>> AssumeRoleCampusAsync([FromBody] AssumeRoleCampusRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await _assumeRoleCampusRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<AssumeRoleCampusCommand>(request);
            var assumeRoleCampusResult = await _sender.Send(command, cancellationToken);
            if (assumeRoleCampusResult.IsFailure)
                return assumeRoleCampusResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var assumeRoleCampus = _mapper.Map<AssumeRoleCampusHttpResponse>(assumeRoleCampusResult.Value);
            ApplySessionAndXsrfToken(assumeRoleCampus.RefreshTokenRaw, assumeRoleCampus.RefreshTokenExpiresAt);
            return Ok(ApiSuccessResponse<AssumeRoleCampusHttpResponse>.From(assumeRoleCampus, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireTemporaryTypeAccessToken)]
        [HttpPost("assume-work-profile")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<AssumeWorkProfileHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<AssumeWorkProfileHttpResponse>>> AssumeWorkProfileAsync([FromBody] AssumeWorkProfileRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await _assumeWorkProfileRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<AssumeWorkProfileCommand>(request);
            var assumeWorkProfileResult = await _sender.Send(command, cancellationToken);
            if (assumeWorkProfileResult.IsFailure)
                return assumeWorkProfileResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var assumeWorkProfile = _mapper.Map<AssumeWorkProfileHttpResponse>(assumeWorkProfileResult.Value);
            ApplySessionAndXsrfToken(assumeWorkProfile.RefreshTokenRaw, assumeWorkProfile.RefreshTokenExpiresAt);
            return Ok(ApiSuccessResponse<AssumeWorkProfileHttpResponse>.From(assumeWorkProfile, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("current-user-role-Campus")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetCurrentUserRoleCampusHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<GetCurrentUserRoleCampusHttpResponse>>> GetCurrentUserRoleCampusAsync(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserRoleCampusQuery();
            var currentUserRoleCampusResult = await _sender.Send(query, cancellationToken);
            if (currentUserRoleCampusResult.IsFailure)
                return currentUserRoleCampusResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var currentUserRoleCampus = _mapper.Map<GetCurrentUserRoleCampusHttpResponse>(currentUserRoleCampusResult.Value);
            return Ok(ApiSuccessResponse<GetCurrentUserRoleCampusHttpResponse>.From(currentUserRoleCampus, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("current-user-work-profile")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<GetCurrentUserWorkProfileHttpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<GetCurrentUserWorkProfileHttpResponse>>> GetCurrentUserWorkProfileAsync(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserWorkProfileQuery();
            var currentUserWorkProfileResult = await _sender.Send(query, cancellationToken);
            if (currentUserWorkProfileResult.IsFailure)
                return currentUserWorkProfileResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var currentUserWorkProfile = _mapper.Map<GetCurrentUserWorkProfileHttpResponse>(currentUserWorkProfileResult.Value);
            return Ok(ApiSuccessResponse<GetCurrentUserWorkProfileHttpResponse>.From(currentUserWorkProfile, HttpContext.TraceIdentifier));
        }

        [Authorize]
        [HttpPost("logout")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> LogoutAsync([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LogoutRequest? request,
            CancellationToken cancellationToken)
        {
            var resolvedRefreshToken = _authSessionCookieService.ResolveRefreshToken(request?.RefreshTokenRaw);
            var command = new LogoutCommand(resolvedRefreshToken);
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





        //[AllowAnonymous]
        //[HttpPost("refresh")]
        //[Consumes("application/json")]
        //[ProducesResponseType(typeof(ApiSuccessResponse<RefreshHttpResponse>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<ApiSuccessResponse<RefreshHttpResponse>>> RefreshAsync([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshRequest? request,
        //    CancellationToken cancellationToken)
        //{
        //    var resolvedRefreshToken = _authSessionCookieService.ResolveRefreshToken(request?.RefreshTokenRaw);
        //    var command = new RefreshCommand(resolvedRefreshToken, request?.DeviceIdentifier);
        //    var refreshResult = await _sender.Send(command, cancellationToken);
        //    if (refreshResult.IsFailure)
        //    {
        //        if (_authSessionCookieService.RequiresCookieClearOnFailure(refreshResult.Errors))
        //            _authSessionCookieService.ClearSessionCookies();
        //        return refreshResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
        //    }
        //    var refresh = _mapper.Map<RefreshHttpResponse>(refreshResult.Value);
        //    ApplySessionAndXsrfToken(refresh.RefreshTokenRaw, refresh.RefreshTokenExpiresAt);
        //    return Ok(ApiSuccessResponse<RefreshHttpResponse>.From(refresh, HttpContext.TraceIdentifier));
        //}

        private void ApplySessionAndXsrfToken(string refreshTokenRaw, DateTime refreshTokenExpiresAt)
        {
            var xsrfToken = _authSessionCookieService.SetSessionCookies(refreshTokenRaw, refreshTokenExpiresAt);
            if (!string.IsNullOrWhiteSpace(xsrfToken))
                Response.Headers["X-XSRF-TOKEN"] = xsrfToken;
        }
    }
}