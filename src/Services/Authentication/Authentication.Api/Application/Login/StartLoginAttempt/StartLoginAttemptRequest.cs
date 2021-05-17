using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.Application.Login.StartLoginAttempt
{
    public class StartLoginAttemptRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        public bool RememberLogin { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}
