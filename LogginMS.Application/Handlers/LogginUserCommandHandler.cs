using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using LogginMS.Application.Commands;
using LogginMS.Application.Dtos;
using LogginMS.Service.Interfaces;
using MediatR;


namespace LogginMS.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, SuccessLogin>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<SuccessLogin> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var token = await _authService.AuthenticateAsync(request.User.UserName, request.User.Password);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Obtener el valor de 'resource_access' del token
            // Extraer el rol "Administrador" de "resource_access"
            var adminRole = jwtToken.Claims
                .FirstOrDefault(c => c.Type == "resource_access")?.Value
                .Split(',') // Si hay múltiples valores de roles, puedes separarlos, si es necesario
                .FirstOrDefault();
            var adminRole2 = adminRole?
            .Split(new[] { "\"roles\":[\"" }, StringSplitOptions.None)[1]
            .Split('"')[0];
            // Extraer el email
            var email = jwtToken.Claims
                .FirstOrDefault(c => c.Type == "email")?.Value;

            Console.WriteLine("Rol de Administrador: " + adminRole2);
            Console.WriteLine("Email: " + email);
            var loginExitoso = new SuccessLogin { username = email, role = adminRole2 };
            return loginExitoso;
        }
    }
}
