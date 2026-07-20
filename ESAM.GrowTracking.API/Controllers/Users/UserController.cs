using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Users.GetUsers;
using ESAM.GrowTracking.API.Controllers.Users.GetUsers.HttpResponses;
using ESAM.GrowTracking.API.Controllers.Users.LockUser;
using ESAM.GrowTracking.API.Controllers.Users.UnlockUser;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application.Features.Users.GetUsers;
using ESAM.GrowTracking.Application.Features.Users.LockUser;
using ESAM.GrowTracking.Application.Features.Users.UnlockUser;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UserController : ControllerBase
    {
        private readonly IValidator<LockUserRequest> _lockUserRequestValidator;
        private readonly IValidator<UnlockUserRequest> _unlockUserRequestValidator;
        private readonly IValidator<GetUsersRequest> _getUsersRequestValidator;
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;

        public UserController(IValidator<LockUserRequest> lockUserRequestValidator, IValidator<UnlockUserRequest> unlockUserRequestValidator, 
            IValidator<GetUsersRequest> getUsersRequestValidator, IMapper mapper, ISender sender, IErrorToHttpMapper errorToHttpMapper)
        {
            ArgumentNullException.ThrowIfNull(lockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(unlockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(getUsersRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            _lockUserRequestValidator = lockUserRequestValidator;
            _unlockUserRequestValidator = unlockUserRequestValidator;
            _getUsersRequestValidator = getUsersRequestValidator;
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("users")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse>>>> GetUsersAsync([FromQuery] GetUsersRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await _getUsersRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var query = _mapper.Map<GetUsersQuery>(request);
            var usersResult = await _sender.Send(query, cancellationToken);
            if (usersResult.IsFailure)
                return usersResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var users = _mapper.Map<PagedHttpResponse<GetUsersHttpResponse>>(usersResult.Value);
            return Ok(ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse>>.From(users, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPost("lock-user")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> LockUserAsync([FromBody] LockUserRequest request, CancellationToken cancellationToken)
        {
            var validation = await _lockUserRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<LockUserCommand>(request);
            var lockUserResult = await _sender.Send(command, cancellationToken);
            if (lockUserResult.IsFailure)
                return lockUserResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPost("unlock-user")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> UnlockUserAsync([FromBody] UnlockUserRequest request, CancellationToken cancellationToken)
        {
            var validation = await _unlockUserRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<UnlockUserCommand>(request);
            var unlockUserResult = await _sender.Send(command, cancellationToken);
            if (unlockUserResult.IsFailure)
                return unlockUserResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }
    }
}