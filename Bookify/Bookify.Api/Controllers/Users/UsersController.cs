using Asp.Versioning;
using Bookify.Application.Users.GetLoggedInUser;
using Bookify.Application.Users.LogInUser;
using Bookify.Application.Users.RegisterUser;
using Bookify.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Users
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    //[ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/users")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("me")]
        /* [Authorize(Roles = Roles.Registered)] -> permissions now are connected to permissions do 
        * it does not makes sense to have both
        */
        // [MapToApiVersion(ApiVersions.V1)]
        [HasPermission(Permissions.UsersRead)]
        public async Task<IActionResult> GetLoggedInUserV1(CancellationToken cancellationToken)
        {
            var query = new GetLoggedInUserQuery();
            var result = await _sender.Send(query, cancellationToken);
            return Ok(result.Value);
        }

        //[HttpGet("me")]
        ///* [Authorize(Roles = Roles.Registered)] -> permissions now are connected to permissions do 
        //* it does not makes sense to have both
        //*/
        //[MapToApiVersion(ApiVersions.V2)]
        //[HasPermission(Permissions.UsersRead)]
        //public async Task<IActionResult> GetLoggedInUserV2(CancellationToken cancellationToken)
        //{
        //    var query = new GetLoggedInUserQuery();
        //    var result = await _sender.Send(query, cancellationToken);
        //    return Ok(result.Value);
        //}

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInUserRequest request, CancellationToken cancellationToken)
        {
            var command = new LogInUserCommand(request.Email, request.Password);

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return Unauthorized(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
