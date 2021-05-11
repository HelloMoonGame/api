using System;
using System.Security.Claims;
using System.Text.Json.Serialization;
using CharacterApi.Application.Characters.DomainServices;
using CharacterApi.Configuration;
using CharacterApi.Domain.Characters;
using CharacterApi.Domain.SeedWork;
using CharacterApi.Infrastructure.Database;
using CharacterApi.Infrastructure.Domain;
using CharacterApi.Infrastructure.Domain.Characters;
using CharacterApi.Infrastructure.Processing;
using CharacterApi.GrpcServices;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CharacterApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CharactersContext>(options =>
                options.UseNpgsql(Environment.ExpandEnvironmentVariables(Configuration.GetConnectionString("DefaultConnection"))));

            services.AddTransient<ISingleCharacterPerUserChecker, SingleCharacterPerUserChecker>();
            services.AddTransient<ICharacterRepository, CharacterRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
            services.AddMediatR(typeof(Startup));
            services.AddMvc().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddGrpc();
            services.AddSwaggerDocumentation();

            services.AddCors(o =>
            {
                o.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                o.AddPolicy("AllowAllGrpc", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
                });
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("default", builder =>
                {
                    builder.RequireClaim("scope", "characterapi");
                });
                options.DefaultPolicy = options.GetPolicy("default") ?? options.DefaultPolicy;
            });
            
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = Configuration["AuthenticationApiUrl"];
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
