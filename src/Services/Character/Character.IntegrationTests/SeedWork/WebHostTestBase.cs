using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Character.Api;
using Character.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Character.IntegrationTests.SeedWork
{
    public abstract class WebHostTestBase : IDisposable
    {
        public const string UserIdWithCharacter = "197d6522-c011-4afa-b2cc-a0d7221d4c6d";
        public const string UserIdWithoutCharacter = "73f18f60-5647-4a3a-abb7-dc71bd933e91";

        protected WebApplicationFactory<Startup> Factory;
        protected HttpClient Client;

        protected WebHostTestBase()
        {
            Factory = new CustomWebApplicationFactory<Startup>();
            Client = Factory.CreateClient();
        }
        
        protected void AuthenticateWith(string userId)
        {
            var token = CreateToken(userId);
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected static string CreateToken(string userId)
        {
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim("sub", userId),
                new Claim("scope", "characterapi")
            });
            return token;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
                Factory?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
