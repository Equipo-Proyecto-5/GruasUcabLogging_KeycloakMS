using LogginMS.Application.Commands;
using LogginMS.Service.Interfaces;
using MediatR;


namespace LogginMS.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var token = await _authService.AuthenticateAsync(request.User.UserName, request.User.Password);
            return token;
        }
    }
}
