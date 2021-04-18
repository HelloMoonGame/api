using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationApi.Data;
using AuthenticationApi.Models.Email;
using AuthenticationApi.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
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
            throw new Exception("invalid return URL");
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
                throw new Exception(result.Errors.First().Description);
            }

            result = await _userManager.AddClaimsAsync(user, new[]{
                new Claim(JwtClaimTypes.Email, user.Email)
            });
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
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

        /// <summary>
        /// Handle postback from email login
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
            var confirmUrl = _configuration["AuthenticationApiUrl"] + Url.Action(nameof(ConfirmLogin),

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email) ?? await CreateUser(model.Email);

                await CancelLoginAttemptsForUserId(user.Id);

                var loginAttempt = new LoginAttempt(user.Id, TimeSpan.FromMinutes(10));
                await _dbContext.LoginAttempts.AddAsync(loginAttempt);
                await _dbContext.SaveChangesAsync();

                var mailService = new MailService();
                    new LoginAttemptConfirmInputModel
                    {
                        Id = loginAttempt.Id,
                        Secret = loginAttempt.Secret
                    });

                if (!user.EmailConfirmed)
                {
                    mailService.SendMail(this, model.Email, new NewUserEmailModel
                    {
                        ConfirmUrl = confirmUrl,
                        Email = user.Email,
                    });
                }
                else
                {
                    mailService.SendMail(this, model.Email, new LoginEmailModel
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

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        /// <summary>
        /// Wait dor approval of the login attempt
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
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

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

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private LoginAttemptConfirmViewModel BuildLoginAttemptConfirmViewModel(LoginAttempt loginAttempt)
        {
            return new LoginAttemptConfirmViewModel
            {
                Id = loginAttempt?.Id,
                ExpiredOrNonExisting = loginAttempt == null || loginAttempt.ExpiryDate < DateTime.UtcNow,
                WasAlreadyConfirmed = loginAttempt?.Accepted ?? false
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Email = context.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Email = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Email = model.Email;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity?.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
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
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}