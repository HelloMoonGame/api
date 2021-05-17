using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Models;
using IdentityServer4.Events;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Application.Logout.DoLogout
{
    public class DoLogoutCommandHandler : IRequestHandler<DoLogoutCommand, LogoutDto>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public DoLogoutCommandHandler(SignInManager<ApplicationUser> signInManager, 
            IEventService events, IIdentityServerInteractionService interaction)
        {
            _signInManager = signInManager;
            _events = events;
            _interaction = interaction;
        }

        public async Task<LogoutDto> Handle(DoLogoutCommand request, CancellationToken cancellationToken)
        {
            var logout = await _interaction.GetLogoutContextAsync(request.LogoutId);

            if (logout != null)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(request.Subject, request.DisplayName));
            }

            return new LogoutDto
            {
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = request.LogoutId
            };
        }
    }
}
