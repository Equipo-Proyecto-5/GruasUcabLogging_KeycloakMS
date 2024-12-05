using LogginMS.Application.Dtos;
using MediatR;

namespace LogginMS.Application.Commands
{
    public class LoginCommand : IRequest<string>
    {
        public UserDto User { get; set; }

        public LoginCommand(UserDto user)
        {
            User = user;
        }
    }
}
