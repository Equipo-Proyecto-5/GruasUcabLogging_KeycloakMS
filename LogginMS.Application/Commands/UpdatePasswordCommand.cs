using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace LogginMS.Application.Commands
{
    public class UpdatePasswordCommand:IRequest
    {
        public UpdatePasswordCommand( string password,string userName)
        {
            Password = password;
            UserName = userName;
        }

        public string Password { get; set; }
        public string UserName { get; set; }
    }
}
