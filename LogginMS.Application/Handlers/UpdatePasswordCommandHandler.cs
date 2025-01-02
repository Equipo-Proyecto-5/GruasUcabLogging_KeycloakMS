using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogginMS.Application.Commands;
using LogginMS.Service.Interfaces;
using MediatR;

namespace LogginMS.Application.Handlers
{
    public class UpdatePasswordCommandHandler:IRequestHandler<UpdatePasswordCommand>
    {
        private readonly IAuthService _authService;

        public UpdatePasswordCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Unit> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            await _authService.UpdatePasswordAsync(request.Password,request.UserName);
            return Unit.Value;
        }
    }
}
