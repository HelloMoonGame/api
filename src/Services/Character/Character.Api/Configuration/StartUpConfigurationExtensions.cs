using Character.Api.GrpcServices;
using Microsoft.AspNetCore.Builder;

namespace Character.Api.Configuration
{
    public static class StartUpConfigurationExtensions
    {
        public static void ConfigureApp(this IApplicationBuilder app, bool isDevelopmentEnvironment)
        {
            if (isDevelopmentEnvironment)
            {
                app.UseDeveloperExceptionPage();
            }

            SeedData.EnsureSeedData(app.ApplicationServices);

            app.UseRouting();

            app.UseGrpcWeb();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LocationService>().EnableGrpcWeb().RequireCors("AllowAllGrpc");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwaggerDocumentation();
        }
    }
}
