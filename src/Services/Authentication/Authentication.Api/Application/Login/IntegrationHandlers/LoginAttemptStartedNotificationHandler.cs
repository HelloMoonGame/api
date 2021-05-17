using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Models;
using Authentication.Api.Models.Email;
using Authentication.Api.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authentication.Api.Application.Login.IntegrationHandlers
{
    public class LoginAttemptStartedNotificationHandler : INotificationHandler<LoginAttemptStartedNotification>
    {
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginAttemptStartedNotificationHandler> _logger;

        public LoginAttemptStartedNotificationHandler(IConfiguration configuration, IMailService mailService, UserManager<ApplicationUser> userManager, ILogger<LoginAttemptStartedNotificationHandler> logger)
        {
            _configuration = configuration;
            _mailService = mailService;
            _userManager = userManager;
            _logger = logger;
        }
        
        public async Task Handle(LoginAttemptStartedNotification notification, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(notification.UserId.ToString());
            var confirmUrl = _configuration["AuthenticationApiUrl"] 
                + $"/Account/ConfirmLogin/{notification.LoginAttemptId}?secret={notification.Secret}";

            if (!user.EmailConfirmed)
            {
                _logger.LogInformation("Send welcome mail");
                _mailService.SendMail(user.Email, new NewUserEmailModel
                {
                    ConfirmUrl = confirmUrl,
                    Email = user.Email,
                });
            }
            else
            {
                _logger.LogInformation("Send login mail");
                _mailService.SendMail(user.Email, new LoginEmailModel
                {
                    ConfirmUrl = confirmUrl,
                    Email = user.Email,
                });
            }
        }
    }
}