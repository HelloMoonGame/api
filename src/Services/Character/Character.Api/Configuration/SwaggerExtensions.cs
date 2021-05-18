using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace Character.Api.Configuration
{
    internal static class SwaggerExtensions
    {
        internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, string authenticationApiUrl)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Character API",
                    Version = "v1",
                    Description = "Every player in the game controls a 'character'. This API allows querying and managing these characters.",
                    Contact = new OpenApiContact
                    {
                        Email = "hello@hellomoon.nl",
                        Name = "Hello Moon",
                        Url = new Uri("https://hellomoon.nl")
                    }
                });

                options.AddSecurityDefinition("OpenID Connect", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authenticationApiUrl + "/connect/authorize"),
                            TokenUrl = new Uri(authenticationApiUrl + "/connect/token"),
                            Scopes = new Dictionary<string, string> { { "characterapi", "Character API" } }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "OpenID Connect"
                            }

                        },
                        new List<string>()
                    }
                });

                options.CustomSchemaIds(type => Regex.Replace(type.Name, "Dto$", ""));

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
                var commentsFile = Path.Combine(baseDirectory, commentsFileName);
                options.IncludeXmlComments(commentsFile);
            });

            return services;
        }

        internal static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    if (httpReq.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues values)
                        && values.Count > 0)
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{values[0]}" } };
                    }
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Character API - Hello Moon";
                c.SwaggerEndpoint("v1/swagger.json", "Character API");
                c.OAuthClientId("docs");
                c.OAuthClientSecret("5b7f084b-9eb4-4f0b-a833-e6a32b9a51bd");
                c.OAuthUsePkce();
            });

            return app;
        }
    }
}