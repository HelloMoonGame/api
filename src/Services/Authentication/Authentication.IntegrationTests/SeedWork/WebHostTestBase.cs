using System;
using System.Net.Http;
using Authentication.Api;
using Authentication.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Authentication.IntegrationTests.SeedWork
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
                MailServiceMock.MailsSent.Clear();
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
