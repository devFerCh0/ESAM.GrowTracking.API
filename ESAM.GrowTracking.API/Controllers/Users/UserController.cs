using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.API.Controllers.Users.DeleteUser;
using ESAM.GrowTracking.API.Controllers.Users.GetUsers;
using ESAM.GrowTracking.API.Controllers.Users.LockUser;
using ESAM.GrowTracking.API.Controllers.Users.ResetUserPassword;
using ESAM.GrowTracking.API.Controllers.Users.RestoreUser;
using ESAM.GrowTracking.API.Controllers.Users.UnlockUser;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application.Features.Users.DeleteUser;
using ESAM.GrowTracking.Application.Features.Users.GetUsers;
using ESAM.GrowTracking.Application.Features.Users.LockUser;
using ESAM.GrowTracking.Application.Features.Users.ResetUserPassword;
using ESAM.GrowTracking.Application.Features.Users.RestoreUser;
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
        private readonly IValidator<GetUsersRequest> _getUsersRequestValidator;
        private readonly IValidator<ResetUserPasswordRequest> _resetUserPasswordRequestValidator;
        private readonly IValidator<DeleteUserRequest> _deleteUserRequestValidator;
        private readonly IValidator<RestoreUserRequest> _restoreUserRequestValidator;
        private readonly IValidator<LockUserRequest> _lockUserRequestValidator;
        private readonly IValidator<UnlockUserRequest> _unlockUserRequestValidator;
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;

        public UserController(IValidator<GetUsersRequest> getUsersRequestValidator, IValidator<ResetUserPasswordRequest> resetUserPasswordRequestValidator,
            IValidator<DeleteUserRequest> deleteUserRequestValidator, IValidator<RestoreUserRequest> restoreUserRequestValidator, 
            IValidator<LockUserRequest> lockUserRequestValidator, IValidator<UnlockUserRequest> unlockUserRequestValidator, IMapper mapper, ISender sender, 
            IErrorToHttpMapper errorToHttpMapper)
        {
            ArgumentNullException.ThrowIfNull(getUsersRequestValidator);
            ArgumentNullException.ThrowIfNull(resetUserPasswordRequestValidator);
            ArgumentNullException.ThrowIfNull(deleteUserRequestValidator);
            ArgumentNullException.ThrowIfNull(restoreUserRequestValidator);
            ArgumentNullException.ThrowIfNull(lockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(unlockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            _getUsersRequestValidator = getUsersRequestValidator;
            _resetUserPasswordRequestValidator = resetUserPasswordRequestValidator;
            _deleteUserRequestValidator = deleteUserRequestValidator;
            _restoreUserRequestValidator = restoreUserRequestValidator;
            _lockUserRequestValidator = lockUserRequestValidator;
            _unlockUserRequestValidator = unlockUserRequestValidator;
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("users")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse.UserHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse.UserHttpResponse>>>> GetUsersAsync([FromQuery] GetUsersRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await _getUsersRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var query = _mapper.Map<GetUsersQuery>(request);
            var usersResult = await _sender.Send(query, cancellationToken);
            if (usersResult.IsFailure)
                return usersResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var users = _mapper.Map<PagedHttpResponse<GetUsersHttpResponse.UserHttpResponse>>(usersResult.Value);
            return Ok(ApiSuccessResponse<PagedHttpResponse<GetUsersHttpResponse.UserHttpResponse>>.From(users, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPatch("reset-user-password")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> ResetUserPasswordAsync([FromBody] ResetUserPasswordRequest request, CancellationToken cancellationToken)
        {
            var validation = await _resetUserPasswordRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<ResetUserPasswordCommand>(request);
            var resetUserPasswordResult = await _sender.Send(command, cancellationToken);
            if (resetUserPasswordResult.IsFailure)
                return resetUserPasswordResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpDelete("delete-user")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> DeleteUserAsync([FromBody] DeleteUserRequest request, CancellationToken cancellationToken)
        {
            var validation = await _deleteUserRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<DeleteUserCommand>(request);
            var deleteUserResult = await _sender.Send(command, cancellationToken);
            if (deleteUserResult.IsFailure)
                return deleteUserResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPatch("restore-user")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> RestoreUserAsync([FromBody] RestoreUserRequest request, CancellationToken cancellationToken)
        {
            var validation = await _restoreUserRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<RestoreUserCommand>(request);
            var restoreUserResult = await _sender.Send(command, cancellationToken);
            if (restoreUserResult.IsFailure)
                return restoreUserResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPatch("lock-user")]
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
        [HttpPatch("unlock-user")]
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