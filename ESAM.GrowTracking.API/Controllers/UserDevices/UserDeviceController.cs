using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Commons;
using ESAM.GrowTracking.API.Controllers.UserDevices.GetUserDevices;
using ESAM.GrowTracking.API.Controllers.UserDevices.UnlockUserDevice;
using ESAM.GrowTracking.API.Extensions;
using ESAM.GrowTracking.API.Responses;
using ESAM.GrowTracking.API.Security;
using ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices;
using ESAM.GrowTracking.Application.Features.UserDevices.UnlockUserDevice;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESAM.GrowTracking.API.Controllers.UserDevices
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class UserDeviceController : ControllerBase
    {
        private readonly IValidator<GetUserDevicesRequest> _getUserDevicesRequestValidator;
        private readonly IValidator<UnlockUserDeviceRequest> _unlockUserDeviceRequestValidator;
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;

        public UserDeviceController(IValidator<GetUserDevicesRequest> getUserDevicesRequestValidator, IValidator<UnlockUserDeviceRequest> unlockUserDeviceRequestValidator, 
            IMapper mapper, ISender sender, IErrorToHttpMapper errorToHttpMapper)
        {
            ArgumentNullException.ThrowIfNull(getUserDevicesRequestValidator);
            ArgumentNullException.ThrowIfNull(unlockUserDeviceRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            _getUserDevicesRequestValidator = getUserDevicesRequestValidator;
            _unlockUserDeviceRequestValidator = unlockUserDeviceRequestValidator;
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpGet("user-devices")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse<PagedHttpResponse<GetUserDevicesHttpResponse.UserDeviceHttpResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse<PagedHttpResponse<GetUserDevicesHttpResponse.UserDeviceHttpResponse>>>> GetUserDevicesAsync(
            [FromQuery] GetUserDevicesRequest request, CancellationToken cancellationToken)
        {
            var validation = await _getUserDevicesRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var query = _mapper.Map<GetUserDevicesQuery>(request);
            var userDevicesResult = await _sender.Send(query, cancellationToken);
            if (userDevicesResult.IsFailure)
                return userDevicesResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            var userDevices = _mapper.Map<PagedHttpResponse<GetUserDevicesHttpResponse.UserDeviceHttpResponse>>(userDevicesResult.Value);
            return Ok(ApiSuccessResponse<PagedHttpResponse<GetUserDevicesHttpResponse.UserDeviceHttpResponse>>.From(userDevices, HttpContext.TraceIdentifier));
        }

        [Authorize(Policy = AuthorizationPolicies.RequireSessionTypeAccessToken)]
        [HttpPatch("unlock-user-device")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status423Locked)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiSuccessResponse>> UnlockUserDeviceAsync([FromBody] UnlockUserDeviceRequest request, CancellationToken cancellationToken)
        {
            var validation = await _unlockUserDeviceRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                return validation.ToRequestErrorsActionResult(HttpContext.TraceIdentifier);
            var command = _mapper.Map<UnlockUserDeviceCommand>(request);
            var unlockUserDeviceResult = await _sender.Send(command, cancellationToken);
            if (unlockUserDeviceResult.IsFailure)
                return unlockUserDeviceResult.ToErrorActionResult(_errorToHttpMapper, HttpContext.TraceIdentifier);
            return Ok(ApiSuccessResponse.From(HttpContext.TraceIdentifier));
        }
    }
}