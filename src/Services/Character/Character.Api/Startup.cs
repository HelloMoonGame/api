﻿using System;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Character.Api.Application.CharacterLocations.DomainServices;
using Character.Api.Application.Characters.DomainServices;
using Character.Api.Configuration;
using Character.Api.Domain.CharacterLocations;
using Character.Api.Domain.Characters;
using Character.Api.Infrastructure.Database;
using Character.Api.Infrastructure.Domain;
using Character.Api.Infrastructure.Domain.CharacterLocations;
using Character.Api.Infrastructure.Domain.Characters;
using Common.Domain.SeedWork;
using Common.Infrastructure.Processing;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Character.Api
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AddDatabase(services);
            services.AddScoped<DbContext>(provider => provider.GetService<CharactersContext>());
            
            AddDependencies(services);
            services.AddMediatR(typeof(Startup));
            services.AddMvc().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddGrpc();
            services.AddSwaggerDocumentation(Configuration["AuthenticationApiUrl"]);

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

            if (!_env.IsDevelopment())
            {
                services.AddApplicationInsightsKubernetesEnricher();
                services.AddApplicationInsightsTelemetry(opt =>
                {
                    opt.EnableActiveTelemetryConfigurationSetup = true;
                });
            }
        }

        protected virtual void AddDatabase(IServiceCollection services)
        {
            services.AddDbContext<CharactersContext>(options =>
                options.UseNpgsql(Environment.ExpandEnvironmentVariables(Configuration.GetConnectionString("DefaultConnection"))));
        }

        public static void AddDependencies(IServiceCollection services)
        {
            services.AddTransient<ISingleCharacterPerUserChecker, SingleCharacterPerUserChecker>();
            services.AddTransient<ISingleLocationPerCharacterChecker, SingleLocationPerCharacterChecker>();
            services.AddTransient<ICharacterRepository, CharacterRepository>();
            services.AddTransient<ICharacterLocationRepository, CharacterLocationRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
            
            services.AddSingleton(s => new DomainNotificationFactory(s.GetService<ILogger<DomainNotificationFactory>>(), typeof(Startup)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureApp(env.IsDevelopment());
        }
    }
}
