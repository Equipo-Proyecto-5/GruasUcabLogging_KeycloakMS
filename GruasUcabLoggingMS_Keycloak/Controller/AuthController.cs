using LogginMS.Application.Commands;
using LogginMS.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
            catch (InvalidOperationException)
            {
                return Forbid();
            }
            catch (Exception ex) {
                return StatusCode(500, "Error al procesar la solicitud");
            }
        }



        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset([FromBody] string userName)
        {
            try
            {
                var result = await _mediator.Send(new PasswordResetCommand(userName));
                return Ok(new { Message = result });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, "Error al procesar la solicitud");
            }
        }

        [HttpPut("{userName}")]
        public async Task<IActionResult> UpdatePassword(string userName,[FromBody] string password)
        {
            try
            {
                var result = await _mediator.Send(new UpdatePasswordCommand(password, userName));
                return Ok();
            }
            catch (Exception) {
                return StatusCode(500, "Error al procesar la solicitud");
            }
        }
    }
}



