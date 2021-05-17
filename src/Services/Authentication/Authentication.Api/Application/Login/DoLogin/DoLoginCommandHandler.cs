using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Domain.Login;
using Authentication.Api.Models;
using Common.Domain.SeedWork;
using IdentityServer4.Events;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Application.Login.DoLogin
{
    public class DoLoginCommandHandler : IRequestHandler<DoLoginCommand, LoginAttemptStatus>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEventService _events;
        private readonly ILoginAttemptRepository _loginAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DoLoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
            IEventService events, ILoginAttemptRepository loginAttemptRepository, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _events = events;
            _loginAttemptRepository = loginAttemptRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginAttemptStatus> Handle(DoLoginCommand request, CancellationToken cancellationToken)
        {
            var loginAttempt = await _loginAttemptRepository.GetByIdAsync(request.LoginAttemptId, cancellationToken);
            if (loginAttempt == null)
                return LoginAttemptStatus.Deleted;

            var loginAttemptDto = LoginAttemptDto.FromLoginAttempt(loginAttempt);
            
            await LoginIfApproved(loginAttemptDto, request.RememberLogin, request.ClientId);

            await DeleteLoginAttemptIfExpiredOrApproved(loginAttemptDto, loginAttempt, cancellationToken);

            return loginAttemptDto.Status;
        }

        private async Task LoginIfApproved(LoginAttemptDto loginAttemptDto, bool rememberLogin, string clientId)
        {
            if (loginAttemptDto.Status == LoginAttemptStatus.Approved)
            {
                var user = await _userManager.FindByIdAsync(loginAttemptDto.UserId.ToString());
                await _signInManager.SignInAsync(user, rememberLogin, "email");
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.Email, user.Id, user.UserName,
                    clientId: clientId));
            }
        }

        private async Task DeleteLoginAttemptIfExpiredOrApproved(LoginAttemptDto loginAttemptDto, 
            LoginAttempt loginAttempt, CancellationToken cancellationToken)
        {
            if (loginAttemptDto.Status == LoginAttemptStatus.Expired || 
                loginAttemptDto.Status == LoginAttemptStatus.Approved)
            {
                _loginAttemptRepository.Delete(loginAttempt);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
        }
    }
}
