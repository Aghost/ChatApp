using System.ComponentModel.DataAnnotations;

namespace ChatApp.Authentication.Configuration.Models.DTOs.Incoming
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
