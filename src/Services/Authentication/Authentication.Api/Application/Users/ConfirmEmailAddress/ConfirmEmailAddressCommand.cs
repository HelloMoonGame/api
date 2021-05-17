using System;
using MediatR;

namespace Authentication.Api.Application.Users.ConfirmEmailAddress
{
    public class ConfirmEmailAddressCommand : IRequest
    {
        public Guid UserId { get; }

        public ConfirmEmailAddressCommand(Guid userId)
        {
            UserId = userId;
        }
    }
}
