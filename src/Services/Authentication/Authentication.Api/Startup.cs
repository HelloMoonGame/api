using Authentication.Api.Configuration;
using Authentication.Api.Data;
using Authentication.Api.Domain.Login;
using Authentication.Api.Infrastructure;
using Authentication.Api.Infrastructure.Domain.Login;
using Authentication.Api.Models;
using Authentication.Api.Services;
using Common.Domain.SeedWork;
using Common.Infrastructure.Domain;
using Common.Infrastructure.Processing;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Authentication.Api
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));
            
            services.AddSingleton<ICorsPolicyService>(container => {
                var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
                return new DefaultCorsPolicyService(logger)
                {
                    AllowAll = true
                };
            });

            services.AddControllersWithViews();

            AddDatabase(services);
            services.AddScoped<DbContext>(provider => provider.GetService<ApplicationDbContext>());

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.ErrorUrl = "/Error/500";
                
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients(Configuration["GameUrl"], Configuration["CharacterApiUrl"]))
                .AddAspNetIdentity<ApplicationUser>();
            
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication();

            var mailConfig = Configuration.GetSection("mail").Get<MailConfig>();
            
            services.AddTransient(_ => mailConfig);
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<ILoginAttemptRepository, LoginAttemptRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

            services.AddSingleton(_ => new DomainNotificationFactory(typeof(Startup)));
            
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        protected virtual void AddDatabase(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(System.Environment.ExpandEnvironmentVariables(Configuration.GetConnectionString("DefaultConnection"))));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ConfigureApp(Environment.IsDevelopment());
        }
    }
}