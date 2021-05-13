using System.Collections.Generic;
using Authentication.Api.Models.Email;
using Authentication.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.IntegrationTests.Mocks
{
    public record Mail
    {
        public string To { get; init; }
        public EmailModel Model { get; init; }
    }
    
    public class MailServiceMock : IMailService
    {
        public static IList<Mail> MailsSent = new List<Mail>();
        
        public void SendMail(Controller controller, string to, EmailModel model)
        {
            MailsSent.Add(new Mail { To = to, Model = model });
        }
    }
}
