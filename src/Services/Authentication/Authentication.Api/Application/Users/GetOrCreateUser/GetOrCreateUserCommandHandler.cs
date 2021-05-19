using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Api.Models;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Application.Users.GetOrCreateUser
{
    public class GetOrCreateUserCommandHandler : IRequestHandler<GetOrCreateUserCommand, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetOrCreateUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> Handle(GetOrCreateUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email))
                return null;
                
            var user = await _userManager.FindByEmailAsync(request.Email) ?? await CreateUser(request.Email);
            return UserDto.FromApplicationUser(user);
        }

        private async Task<ApplicationUser> CreateUser(string email)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new CouldNotCreateUserException(result.Errors.First().Description);
            }

            result = await _userManager.AddClaimsAsync(user, new[]{
                new Claim(JwtClaimTypes.Email, user.Email)
            });
            if (!result.Succeeded)
            {
                throw new CouldNotCreateUserException(result.Errors.First().Description);
            }

            return user;
        }
    }
}
