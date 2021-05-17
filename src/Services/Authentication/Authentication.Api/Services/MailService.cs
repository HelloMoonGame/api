using System;
using System.IO;
using System.Threading.Tasks;
using Authentication.Api.Configuration;
using Authentication.Api.Controllers;
using Authentication.Api.Models.Email;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using MimeKit;

namespace Authentication.Api.Services
{
    public interface IMailService
    {
        public void SendMail(string to, EmailModel model);
    }

    public class MailService : IMailService
    {
        private readonly MailConfig _config;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public MailService(MailConfig config, ICompositeViewEngine viewEngine, 
            ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _config = config;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
        
        public async void SendMail(string to, EmailModel model)
        {
            var context = GetActionContext();
            var viewEngineResult = _viewEngine.FindView(context, "Email/" + model.ViewName, false);
            if (viewEngineResult == null || !viewEngineResult.Success)
                throw new FileNotFoundException("No mail template found for " + model.ViewName);
            
            string content;

            await using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    context,
                    viewEngineResult.View,
                    new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        context.HttpContext,
                        _tempDataProvider),
                    writer,
                    new HtmlHelperOptions());

                await viewEngineResult.View.RenderAsync(viewContext);

                content = writer.GetStringBuilder().ToString();
            }

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_config.FromName, _config.FromMailAddress));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = model.Subject;
            message.Body = new TextPart("html") { Text = content };

            await DeliverMail(_config.DeliveryMethod, message);
        }

        private Task DeliverMail(DeliveryMethod configDeliveryMethod, MimeMessage message)
        {
            return configDeliveryMethod switch
            {
                DeliveryMethod.SpecifiedPickupDirectory => SaveToPickupDirectory(message),
                DeliveryMethod.Network => SendViaNetwork(message),
                _ => throw new ArgumentOutOfRangeException(nameof(configDeliveryMethod),
                    "Unsupported delivery method, only SpecifiedPickupDirectory or Network are allowed")
            };
        }

        private async Task SaveToPickupDirectory(MimeMessage message)
        {
            var path = Path.Combine(_config.PickupDirectoryLocation, Guid.NewGuid() + ".eml");

            await using var stream = new FileStream(path, FileMode.CreateNew);
            await message.WriteToAsync(stream);
        }
        
        private async Task SendViaNetwork(MimeMessage message)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort);
            if (!string.IsNullOrEmpty(_config.SmtpUserName) || !string.IsNullOrEmpty(_config.SmtpPassword))
                await client.AuthenticateAsync(_config.SmtpUserName, _config.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
