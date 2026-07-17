using AutoMapper;
using ESAM.GrowTracking.API.Abstractions.Mappers;
using ESAM.GrowTracking.API.Controllers.Users.LockUser;
using ESAM.GrowTracking.API.Controllers.Users.UnlockUser;
using FluentValidation;
using MediatR;
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
        private readonly IMapper _mapper;
        private readonly ISender _sender;
        private readonly IErrorToHttpMapper _errorToHttpMapper;

        public UserController(IValidator<LockUserRequest> lockUserRequestValidator, IValidator<UnlockUserRequest> unlockUserRequestValidator, IMapper mapper, ISender sender, 
            IErrorToHttpMapper errorToHttpMapper)
        {
            ArgumentNullException.ThrowIfNull(lockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(unlockUserRequestValidator);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(errorToHttpMapper);
            _lockUserRequestValidator = lockUserRequestValidator;
            _unlockUserRequestValidator = unlockUserRequestValidator;
            _mapper = mapper;
            _sender = sender;
            _errorToHttpMapper = errorToHttpMapper;
        }
    }
}