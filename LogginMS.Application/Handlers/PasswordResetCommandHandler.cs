using LogginMS.Application.Commands;
using LogginMS.Service.Interfaces;
using MediatR;


namespace LogginMS.Application.Handlers
{
    public class PasswordResetCommandHandler : IRequestHandler<PasswordResetCommand, string>
    {
        private readonly IAuthService _authService;

        public PasswordResetCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<string> Handle(PasswordResetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Llama al servicio para iniciar la recuperación
                await _authService.RequestPasswordResetAsync(request.Username);

                // Retorna un mensaje de éxito
                return "Se ha enviado un correo para restablecer la contraseña.";
            }
            catch (Exception ex)
            {
                // Retorna el mensaje de error
                return $"Error al intentar recuperar la contraseña: {ex.Message}";
            }
        }
    }
}
