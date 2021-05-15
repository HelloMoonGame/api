﻿using System;
using System.IO;
using System.Threading.Tasks;
using Authentication.Api.Configuration;
using Authentication.Api.Models.Email;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MimeKit;

namespace Authentication.Api.Services
{
    public interface IMailService
    {
        public void SendMail(Controller controller, string to, EmailModel model);
    }

    public class MailService : IMailService
    {
        private readonly MailConfig _config;

        public MailService(MailConfig config)
        {
            _config = config;
        }
        
        public async void SendMail(Controller controller, string to, EmailModel model)
        {
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var view = viewEngine?.FindView(controller.ControllerContext, "Email/" + model.ViewName, true);
            if (view == null)
                throw new FileNotFoundException("No mail template found for " + model.ViewName);
            
            controller.ViewData.Model = model;
            string content;

            await using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    view.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await view.View.RenderAsync(viewContext);

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
            switch (configDeliveryMethod)
            {
                case DeliveryMethod.SpecifiedPickupDirectory: return SaveToPickupDirectory(message);
                case DeliveryMethod.Network: return SendViaNetwork(message);
                default:
                    throw new ArgumentOutOfRangeException(nameof(configDeliveryMethod), "Unsupported delivery method, only SpecifiedPickupDirectory or Network are allowed");
            }
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
    }
}
