using System.Net;
using System.Threading.Tasks;
using Authentication.Api.Infrastructure;
using Authentication.Api.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Authentication.Api.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;

        public ErrorController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment)
        {
            _interaction = interaction;
            _environment = environment;
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        [Route("/Error/500")]
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identity server
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    message.ErrorDescription = null;
                }
            }

            var output = View("Error", vm);
            output.StatusCode = (int) HttpStatusCode.InternalServerError;
            return output;
        }

        /// <summary>
        /// Shows the page not found error page
        /// </summary>
        [Route("/Error/404")]
        public IActionResult PageNotFound()
        {
            var output = View("NotFound");
            output.StatusCode = (int) HttpStatusCode.NotFound;
            return output;
        }
    }
}