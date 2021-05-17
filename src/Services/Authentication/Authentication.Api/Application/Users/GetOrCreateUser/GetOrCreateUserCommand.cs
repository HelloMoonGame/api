using MediatR;

namespace Authentication.Api.Application.Users.GetOrCreateUser
{
    public class GetOrCreateUserCommand : IRequest<UserDto>
    {
        public string Email { get; }

        public GetOrCreateUserCommand(string email)
        {
            Email = email;
        }
    }
}
