using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Authentication.Api.Data;
using Authentication.Api.Infrastructure;
using Authentication.Api.InputModels;
using Authentication.Api.Models;
using Authentication.Api.Models.Email;
using Authentication.Api.Services;
using Authentication.Api.ViewModels;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Authentication.Api.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly IMailService _mailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IIdentityServerInteractionService interaction,
            IEventService events,
            IMailService mailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _interaction = interaction;
            _events = events;
            _mailService = mailService;
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
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                return await CancelLogin(context, model.ReturnUrl);
            }

            var user = await _userManager.FindByEmailAsync(model.Email) ?? await CreateUser(model.Email);

            await CancelLoginAttemptsForUserId(user.Id);
            var loginAttempt = await CreateLoginAttempt(user.Id);

            var confirmUrl = _configuration["AuthenticationApiUrl"] + Url.Action(nameof(ConfirmLogin),
                new LoginAttemptConfirmInputModel
                {
                    Id = loginAttempt.Id,
                    Secret = loginAttempt.Secret
                });

            if (!user.EmailConfirmed)
            {
                _mailService.SendMail(this, model.Email, new NewUserEmailModel
                {
                    ConfirmUrl = confirmUrl,
                    Email = user.Email,
                });
            }
            else
            {
                _mailService.SendMail(this, model.Email, new LoginEmailModel
                {
                    ConfirmUrl = confirmUrl,
                    Email = user.Email,
                });
            }

            return RedirectToAction("WaitForLoginApproval", new LoginAttemptInputModel
            {
                Id = loginAttempt.Id,
                RememberLogin = model.RememberLogin,
                ReturnUrl = model.ReturnUrl
            });
        }

        private IActionResult RedirectToReturnUrl(AuthorizationRequest context, string returnUrl)
        {
            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
            if (context != null && context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage("Redirect", returnUrl);
            }

            return Redirect(returnUrl);
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

        private async Task<IActionResult> CancelLogin(AuthorizationRequest context, string returnUrl)
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                return RedirectToReturnUrl(context, returnUrl);
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("/");
            }
        }

        private async Task<ApplicationUser> CreateUser(string email)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new AuthenticationException(result.Errors.First().Description);
            }

            result = await _userManager.AddClaimsAsync(user, new[]{
                new Claim(JwtClaimTypes.Email, user.Email)
            });
            if (!result.Succeeded)
            {
                throw new AuthenticationException(result.Errors.First().Description);
            }

            return user;
        }

        private async Task DeleteLoginAttempt(LoginAttempt loginAttempt)
        {
            _dbContext.LoginAttempts.Remove(loginAttempt);
            await _dbContext.SaveChangesAsync();
        }

        private async Task CancelLoginAttemptsForUserId(string userId)
        {
            var loginAttempts = await _dbContext.LoginAttempts.Where(l => l.UserId == userId).ToListAsync();
            if (loginAttempts.Any())
            {
                loginAttempts.ForEach(l => _dbContext.Remove(l));
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<LoginAttempt> CreateLoginAttempt(string userId)
        {
            var loginAttempt = new LoginAttempt(userId, TimeSpan.FromMinutes(10));
            await _dbContext.LoginAttempts.AddAsync(loginAttempt);
            await _dbContext.SaveChangesAsync();

            return loginAttempt;
        }

        /// <summary>
        /// Wait for approval of the login attempt
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> WaitForLoginApproval(LoginAttemptInputModel model, string button)
        {
            if (model == null)
                return BadRequest();

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            var loginAttempt = await _dbContext.LoginAttempts.FindAsync(model.Id);
            if (loginAttempt == null)
                return await CancelLogin(context, model.ReturnUrl);

            var user = await _userManager.FindByIdAsync(loginAttempt.UserId);
            if (user == null)
                return await CancelLogin(context, model.ReturnUrl);

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
        public async Task<IActionResult> CheckLoginApproval(LoginAttemptInputModel model)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var returnUrl = context != null ? model.ReturnUrl : GetLocalReturnUrl(model.ReturnUrl);

            // validate login attempt
            var loginAttempt = await _dbContext.LoginAttempts.FindAsync(model.Id);
            if (loginAttempt == null || DateTime.UtcNow > loginAttempt.ExpiryDate)
            {
                if (context != null) await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                return Json(new
                {
                    Expired = true,
                    ReturnUrl = returnUrl
                });
            }

            // if login attempt is accepted, forward to returnUrl
            if (loginAttempt.Accepted)
            {
                var user = await _userManager.FindByIdAsync(loginAttempt.UserId);
                await _signInManager.SignInAsync(user, model.RememberLogin, "email");
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.Email, user.Id, user.UserName,
                    clientId: context?.Client.ClientId));

                await DeleteLoginAttempt(loginAttempt);

                return Json(new
                {
                    Approved = true,
                    ReturnUrl = returnUrl
                });
            }

            // please retry later
            return Json(new
            {
                Approved = false
            });
        }

        /// <summary>
        /// Handle click from login-email
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfirmLogin(LoginAttemptConfirmInputModel model)
        {
            var loginAttempt = await _dbContext.LoginAttempts.SingleOrDefaultAsync(l => l.Id == model.Id && l.Secret == model.Secret);
            var viewModel = BuildLoginAttemptConfirmViewModel(loginAttempt);
            
            return View(viewModel);
        }

        /// <summary>
        /// Handle click from login-email
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmLoginFinish(LoginAttemptConfirmInputModel model, string button)
        {
            var loginAttempt = await _dbContext.LoginAttempts.SingleOrDefaultAsync(l => l.Id == model.Id && l.Secret == model.Secret);
            var viewModel = BuildLoginAttemptConfirmViewModel(loginAttempt);

            // the user clicked the "cancel" button
            if (button != "yes")
            {
                await DeleteLoginAttempt(loginAttempt);
            }
            else if (loginAttempt != null && !viewModel.WasAlreadyConfirmed && !viewModel.ExpiredOrNonExisting)
            {
                loginAttempt.Accepted = true;
                var user = await _dbContext.Users.SingleAsync(u => u.Id == loginAttempt.UserId);
                user.EmailConfirmed = true;
                await _dbContext.SaveChangesAsync();
                
                viewModel.Accepted = true;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var vm = new LogoutInputModel { LogoutId = logoutId };
            if (User?.Identity?.IsAuthenticated != true)
                return await Logout(vm);
            
            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity?.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }
            
            return View("LoggedOut", vm);
        }
        
        private static LoginAttemptConfirmViewModel BuildLoginAttemptConfirmViewModel(LoginAttempt loginAttempt)
        {
            return new()
            {
                Id = loginAttempt?.Id,
                ExpiredOrNonExisting = loginAttempt == null || loginAttempt.ExpiryDate < DateTime.UtcNow,
                WasAlreadyConfirmed = loginAttempt?.Accepted ?? false
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

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity?.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout && vm.LogoutId == null)
                    {
                        // if there's no current logout context, we need to create one
                        // this captures necessary info from the current logged in user
                        // before we signout and redirect away to the external IdP for signout
                        vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                    }
                }
            }

            return vm;
        }
    }
}