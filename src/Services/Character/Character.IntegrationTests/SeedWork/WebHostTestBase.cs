using System;
using System.Net.Http;
using Character.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Character.IntegrationTests.SeedWork
{
    public abstract class WebHostTestBase : IDisposable
    {
        protected WebApplicationFactory<Startup> Factory;
        protected HttpClient Client;

        protected WebHostTestBase()
        {
            Factory = new CustomWebApplicationFactory<Startup>();
            Client = Factory.CreateClient();
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
