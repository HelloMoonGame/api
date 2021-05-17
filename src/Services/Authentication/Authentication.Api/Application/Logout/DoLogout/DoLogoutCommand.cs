using System;
using MediatR;

namespace Authentication.Api.Application.Logout.DoLogout
{
    public class DoLogoutCommand : IRequest<LogoutDto>
    {
        public string LogoutId { get; }

        public string Subject { get; }

        public string DisplayName { get; }
        
        public DoLogoutCommand(string logoutId, string subject, string displayName)
        {
            LogoutId = logoutId;
            Subject = subject;
            DisplayName = displayName;
        }
    }
}
