using System;
using Authentication.Api.Models;

namespace Authentication.Api.Application.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        
        public static UserDto FromApplicationUser(ApplicationUser applicationUser)
        {
            return new()
            {
                Id = Guid.Parse(applicationUser.Id)
            };
        }
    }
}
