using MediatR;


namespace LogginMS.Application.Commands
{
    public class PasswordResetCommand : IRequest<string>
    {
        public string Username { get; set; }

        public PasswordResetCommand(string username)
        {
            Username = username;
        }
    }

}
