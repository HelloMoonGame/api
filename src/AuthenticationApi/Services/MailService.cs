using System.IO;
using System.Net.Mail;
using AuthenticationApi.Models.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AuthenticationApi.Services
{
    public class MailService
    {
        public async void SendMail(Controller controller, string to, EmailModel model)
        {
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var view = viewEngine.FindView(controller.ControllerContext, "Email/" + model.ViewName, true);
            controller.ViewData.Model = model;
            var content = "";
            
            using (var writer = new StringWriter())
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
            
            using var smtp = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = @"c:\maildump"
            };
            
            var message = new MailMessage
            {
                Body = content,
                IsBodyHtml = true,
                Subject = model.Subject,
                From = new MailAddress("no-reply@hellomoon.nl", "Hello Moon")
            };
            
            message.To.Add(to);
            await smtp.SendMailAsync(message);
        }
    }
}
