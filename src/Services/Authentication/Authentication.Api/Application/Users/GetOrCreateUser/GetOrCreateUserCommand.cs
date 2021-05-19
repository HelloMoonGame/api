using System;
using MediatR;

namespace Authentication.Api.Application.Users.GetOrCreateUser
{
    public class GetOrCreateUserCommand : IRequest<UserDto>
    {
        public string Email { get; }

        public GetOrCreateUserCommand(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));
            Email = email;
        }
    }
}
