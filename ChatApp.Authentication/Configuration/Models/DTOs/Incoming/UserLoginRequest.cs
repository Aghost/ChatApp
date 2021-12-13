using System.ComponentModel.DataAnnotations;

namespace ChatApp.Authentication.Configuration.Models.DTOs.Incoming
{
    public class UserLoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
