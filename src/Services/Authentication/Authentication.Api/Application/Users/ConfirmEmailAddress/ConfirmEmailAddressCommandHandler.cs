using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Application.Users.ConfirmEmailAddress
{
    public class ConfirmEmailAddressCommandHandler : IRequestHandler<ConfirmEmailAddressCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailAddressCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<Unit> Handle(ConfirmEmailAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            
            return Unit.Value;
        }
    }
}
