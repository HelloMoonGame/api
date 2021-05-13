// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Claims;
using Authentication.Api.Data;
using Authentication.Api.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Authentication.Api
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            MigrateDatabase(context);
        }
        private static void MigrateDatabase(ApplicationDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Database dependency could not be resolved");
            context.Database.Migrate();
        }
    }
}
