using Microsoft.AspNetCore.Builder;

namespace Authentication.Api.Infrastructure
{
    public static class StartUpConfigurationExtensions
    {

        public static void ConfigureApp(this IApplicationBuilder app, bool isDevelopmentEnvironment)
        {
            if (isDevelopmentEnvironment)
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
            }

            app.UseNotFoundHandler("/Error/404");

            SeedData.EnsureSeedData(app.ApplicationServices);

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}
