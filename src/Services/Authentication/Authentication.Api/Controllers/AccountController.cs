using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Application.Login;
using Authentication.Api.Application.Login.ApproveLoginAttempt;
using Authentication.Api.Application.Login.DoLogin;
using Authentication.Api.Application.Login.GetLoginAttempt;
using Authentication.Api.Application.Login.RejectLoginAttempt;
using Authentication.Api.Application.Login.StartLoginAttempt;
using Authentication.Api.Application.Logout;
using Authentication.Api.Application.Logout.DoLogout;
using Authentication.Api.Application.Users.GetOrCreateUser;
using Authentication.Api.Infrastructure;
using Authentication.Api.InputModels;
using Authentication.Api.ViewModels;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IMediator _mediator;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IMediator mediator)
        {
            _interaction = interaction;
            _mediator = mediator;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var vm = await BuildLoginViewModelAsync(returnUrl);
            
            return View(vm);
        }

        /// <summary>
        /// Try to login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StartLoginAttemptRequest model, string button, CancellationToken cancellationToken)
        {
            // the user clicked the "cancel" button
            if (button != "login")
                return await CancelLogin(model.ReturnUrl); 

            var user = await _mediator.Send(new GetOrCreateUserCommand(model.Email), cancellationToken);

            var loginAttempt = await _mediator.Send(
                new StartLoginAttemptCommand(
                    user.Id, 
                    model.RememberLogin, 
                    model.ReturnUrl), 
                cancellationToken);
            
            return RedirectToAction("WaitForLoginApproval", new LoginAttemptInputModel
            {
                Id = loginAttempt.Id,
                RememberLogin = model.RememberLogin,
                ReturnUrl = model.ReturnUrl
            });
        }

        private string GetLocalReturnUrl(string returnUrl)
        {
            // request for a local page
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            if (string.IsNullOrEmpty(returnUrl))
            {
                return "/";
            }

            // user might have clicked on a malicious link - should be logged
            throw new AuthenticationException("invalid return URL");
        }

        private async Task<IActionResult> CancelLogin(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context != null)
            {
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);
                if (context.IsNativeClient())
                {
                    // For native clients it is better to show a nice text and redirect with Javascript
                    return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
                }
                return Redirect(returnUrl);
            }
            
            return Redirect("/");
        }
        
        /// <summary>
        /// Wait for approval of the login attempt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> WaitForLoginApproval(LoginAttemptInputModel model, CancellationToken cancellationToken)
        {
            if (model == null)
                return BadRequest();
            
            var loginAttempt = await _mediator.Send(new GetLoginAttemptQuery(model.Id), cancellationToken);
            if (loginAttempt == null)
                return await CancelLogin(model.ReturnUrl);
            
            return View("WaitForLoginApproval", new LoginAttemptViewModel
            {
                ReturnUrl = model.ReturnUrl,
                RememberLogin = model.RememberLogin,
                Id = loginAttempt.Id,
                ExpiryDate = loginAttempt.ExpiryDate
            });
        }

        /// <summary>
        /// Check if login is already approved
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckLoginApproval(LoginAttemptInputModel model, CancellationToken cancellationToken)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var returnUrl = context != null ? model.ReturnUrl : GetLocalReturnUrl(model.ReturnUrl);

            var loginAttempt = await _mediator.Send(new GetLoginAttemptQuery(model.Id), cancellationToken);
            if ((loginAttempt == null || loginAttempt.Status == LoginAttemptStatus.Expired) && context != null)
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);
            
            if (loginAttempt?.Status == LoginAttemptStatus.Approved)
                await _mediator.Send(new DoLoginCommand(loginAttempt.Id, model.RememberLogin, context?.Client.ClientId), cancellationToken);
            
            return Json(new
            {
                Approved = loginAttempt?.Status == LoginAttemptStatus.Approved,
                Expired = loginAttempt == null || loginAttempt.Status == LoginAttemptStatus.Expired,
                ReturnUrl = returnUrl
            });
        }

        /// <summary>
        /// Handle click from login-email
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfirmLogin(LoginAttemptConfirmInputModel model, CancellationToken cancellationToken)
        {
            var loginAttemptId = model.Id.GetValueOrDefault();
            var loginAttempt = await _mediator.Send(new GetLoginAttemptQuery(loginAttemptId), cancellationToken);
            if (loginAttempt?.Secret != model.Secret)
                loginAttempt = null;
            
            var viewModel = BuildLoginAttemptConfirmViewModel(loginAttemptId, loginAttempt);
            
            return View(viewModel);
        }

        /// <summary>
        /// Handle click from login-email
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmLoginFinish(LoginAttemptConfirmInputModel model, string button, CancellationToken cancellationToken)
        {
            var loginAttemptId = model.Id.GetValueOrDefault();
            if (button != "yes")
            {
                var loginAttempt = await _mediator.Send(new RejectLoginAttemptCommand(loginAttemptId, model.Secret), cancellationToken);
                return View(loginAttempt != null ? LoginAttemptStatus.Deleted : null);
            } else {
                var loginAttempt = await _mediator.Send(new ApproveLoginAttemptCommand(loginAttemptId, model.Secret), cancellationToken);
                return View(loginAttempt?.Status);
            }
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public IActionResult Logout(string logoutId)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return View("LoggedOut", new LogoutDto());

            var vm = new LogoutInputModel { LogoutId = logoutId };
            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            var vm = new LogoutDto();
            var authenticated = User?.Identity?.IsAuthenticated == true;

            if (authenticated)
            {
                if (string.IsNullOrEmpty(model.LogoutId))
                {
                    var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                    if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider && await HttpContext.GetSchemeSupportsSignOutAsync(idp))
                    {
                        model.LogoutId = await _interaction.CreateLogoutContextAsync();
                    }
                }
                vm = await _mediator.Send(new DoLogoutCommand(model.LogoutId, User.GetSubjectId(), User.GetDisplayName()));
            }
            
            return View("LoggedOut", vm);
        }
        
        private static LoginAttemptConfirmViewModel BuildLoginAttemptConfirmViewModel(Guid id, LoginAttemptDto loginAttempt)
        {
            return new()
            {
                Id = id,
                ExpiredOrNonExisting = loginAttempt == null || loginAttempt.Status == LoginAttemptStatus.Expired,
                WasAlreadyConfirmed = loginAttempt?.Status == LoginAttemptStatus.Approved
            };
        }

        private async Task<LoginInputModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            
            return new LoginInputModel
            {
                ReturnUrl = returnUrl,
                Email = context?.LoginHint
            };
        }
    }
}