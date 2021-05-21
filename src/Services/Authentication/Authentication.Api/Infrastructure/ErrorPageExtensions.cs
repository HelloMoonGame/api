using Microsoft.AspNetCore.Builder;

namespace Authentication.Api.Infrastructure
{
    public static class ErrorPageExtensions
    {
        public static void UseNotFoundHandler(this IApplicationBuilder app, string path)
        {
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Request.Path = path;
                    await next();
                }
            });
        }
    }
}
