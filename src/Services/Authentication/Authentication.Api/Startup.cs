using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Authentication.Api.Configuration;
using Authentication.Api.Data;
using Authentication.Api.Domain.Login;
using Authentication.Api.Infrastructure;
using Authentication.Api.Infrastructure.Domain.Login;
using Authentication.Api.Models;
using Authentication.Api.Services;
using Common.Domain.SeedWork;
using Common.Domain.SharedKernel;
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
using Org.BouncyCastle.Security.Certificates;
using Serilog;

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

            var certificateFile = Path.Combine(".", "certs", "certificate.pfx");
            var directoryInfo = new FileInfo(certificateFile).Directory;
            if (directoryInfo != null)
                Directory.CreateDirectory(directoryInfo.FullName);
            var certificate = LoadOrCreateCertificate(certificateFile);
            builder.AddSigningCredential(certificate)
                .AddValidationKey(certificate);

            services.AddAuthentication();

            var mailConfig = Configuration.GetSection("mail").Get<MailConfig>();
            
            services.AddTransient(_ => mailConfig);
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<ILoginAttemptRepository, LoginAttemptRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

            services.AddSingleton(s => new DomainNotificationFactory(s.GetService<ILogger<DomainNotificationFactory>>(), typeof(Startup)));

            services.AddDatabaseDeveloperPageExceptionFilter();

            if (!Environment.IsDevelopment())
            {
                services.AddApplicationInsightsKubernetesEnricher();
                services.AddApplicationInsightsTelemetry(opt =>
                {
                    opt.EnableActiveTelemetryConfigurationSetup = true;
                });
            }
        }

        private X509Certificate2 LoadOrCreateCertificate(string certificateFile)
        {
            X509Certificate2 certificate;
            try
            {
                certificate = new X509Certificate2(certificateFile);
                if (certificate.NotBefore.ToUniversalTime() > SystemClock.Now ||
                    certificate.NotAfter.ToUniversalTime() < SystemClock.Now)
                {
                    Log.Logger.Warning("Certificate only valid between {ValidFrom} and {ValidTill}", 
                        certificate.NotBefore.ToUniversalTime(), 
                        certificate.NotAfter.ToUniversalTime());
                    throw new CertificateExpiredException();
                }
            }
            catch (Exception e)
            {
                Log.Logger.Warning(e, "Creating new certificate");
                certificate = GenerateSelfSignedCertificate();
                File.WriteAllBytes(certificateFile, certificate.Export(X509ContentType.Pfx));
            }

            return certificate;
        }

        public static X509Certificate2 GenerateSelfSignedCertificate()
        {
            X500DistinguishedName distinguishedName = new X500DistinguishedName("CN=Authentication");

            using RSA rsa = RSA.Create(2048);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new ("1.3.6.1.5.5.7.3.2") }, false));

            return request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddYears(10)));
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