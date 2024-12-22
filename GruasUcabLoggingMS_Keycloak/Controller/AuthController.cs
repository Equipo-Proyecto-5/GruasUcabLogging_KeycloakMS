using LogginMS.Application.Commands;
using LogginMS.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GruasUcabLoggingMS_Keycloak.Controller
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto request)
        {
            var command = new LoginCommand(request);

            try
            {
                var tokenResponse = await _mediator.Send(command);
                return Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }


        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset([FromBody] UserDtoPasswordReset request)
        {
            var result = await _mediator.Send(new PasswordResetCommand(request.UserName));
            return Ok(new { Message = result });
        }
    }
}

