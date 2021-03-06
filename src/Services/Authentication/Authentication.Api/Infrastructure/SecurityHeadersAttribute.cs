using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Authentication.Api.Infrastructure
{
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result;
            if (result is ViewResult)
            {
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                context.HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                context.HttpContext.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                var csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self'; upgrade-insecure-requests; style-src 'self' https://fonts.googleapis.com/ https://unpkg.com/material-components-web@latest/; font-src https://fonts.gstatic.com; script-src 'self' https://unpkg.com/material-components-web@latest/";
                context.HttpContext.Response.Headers["Content-Security-Policy"] = csp;
                context.HttpContext.Response.Headers["X-Content-Security-Policy"] = csp;

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                context.HttpContext.Response.Headers["Referrer-Policy"] = "no-referrer";
            }
        }
    }
}
