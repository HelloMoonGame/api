using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Authentication.IntegrationTests.SeedWork
{
    public abstract class WebHostTestBase : IDisposable
    {
        protected TestServer Server;
        protected HttpClient Client;

        public WebHostTestBase()
        {

            var webHostBuilder =
                new WebHostBuilder()
                    .UseEnvironment("Test")
                    .UseStartup<TestStartup>();
            
            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server?.Dispose();
                Client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
