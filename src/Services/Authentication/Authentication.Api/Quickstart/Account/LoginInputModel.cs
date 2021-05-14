using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Quickstart.Account
{
    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}