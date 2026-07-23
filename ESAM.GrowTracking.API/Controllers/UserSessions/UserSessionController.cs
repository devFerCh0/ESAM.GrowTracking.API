using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSession;
using ESAM.GrowTracking.API.Controllers.UserSessions.CloseUserSessions;
using ESAM.GrowTracking.API.Controllers.UserSessions.GetUserSessions;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSession;
using ESAM.GrowTracking.Application.Features.UserSessions.CloseUserSessions;
using ESAM.GrowTracking.Application.Features.UserSessions.GetUserSessions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Controllers.UserSessions
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UserSessionController : ControllerBase
    {
        private readonly IValidator<GetUserSessionsRequest> _getUserSessionsRequestValidator;
        private readonly IValidator<CloseUserSessionRequest> _closeUserSessionRequestValidator;
        private readonly IValidator<CloseUserSessionsRequest> _closeUserSessionsRequestValidator;
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;

        public UserSessionController(IValidator<GetUserSessionsRequest> getUserSessionsRequestValidator, IValidator<CloseUserSessionRequest> closeUserSessionRequestValidator,
            IValidator<CloseUserSessionsRequest> closeUserSessionsRequestValidator, IMapper mapper, ISender sender, IErrorToHttpMapper errorToHttpMapper)
        {
            ArgumentNullException.ThrowIfNull(getUserSessionsRequestValidator);
            ArgumentNullException.ThrowIfNull(closeUserSessionRequestValidator);
            ArgumentNullException.ThrowIfNull(closeUserSessionsRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            _getUserSessionsRequestValidator = getUserSessionsRequestValidator;
            _closeUserSessionRequestValidator = closeUserSessionRequestValidator;
            _closeUserSessionsRequestValidator = closeUserSessionsRequestValidator;
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("user-sessions")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<PagedHttpResponse<GetUserSessionsHttpResponse.UserSessionHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<PagedHttpResponse<GetUserSessionsHttpResponse.UserSessionHttpResponse>>>> GetUserSessionsAsync(
            [FromQuery] GetUserSessionsRequest request, CancellationToken cancellationToken)
        {
            var validation = await _getUserSessionsRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var query = _mapper.Map<GetUserSessionsQuery>(request);
            var userSessionsResult = await _sender.Send(query, cancellationToken);
            if (userSessionsResult.IsFailure)
                return userSessionsResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var userSessions = _mapper.Map<PagedHttpResponse<GetUserSessionsHttpResponse.UserSessionHttpResponse>>(userSessionsResult.Value);
            return Ok(ApiSuccessResponse<PagedHttpResponse<GetUserSessionsHttpResponse.UserSessionHttpResponse>>.From(userSessions, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpDelete("close-user-session")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> CloseUserSessionAsync([FromBody] CloseUserSessionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _closeUserSessionRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<CloseUserSessionCommand>(request);
            var closeUserSessionResult = await _sender.Send(command, cancellationToken);
            if (closeUserSessionResult.IsFailure)
                return closeUserSessionResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpDelete("close-user-sessions")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> CloseUserSessionsAsync([FromBody] CloseUserSessionsRequest request, CancellationToken cancellationToken)
        {
            var validation = await _closeUserSessionsRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<CloseUserSessionsCommand>(request);
            var closeUserSessionsResult = await _sender.Send(command, cancellationToken);
            if (closeUserSessionsResult.IsFailure)
                return closeUserSessionsResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }
    }
}