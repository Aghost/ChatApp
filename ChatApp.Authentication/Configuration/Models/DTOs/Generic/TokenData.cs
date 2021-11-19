using System.ComponentModel.DataAnnotations;

namespace ChatApp.Authentication.Configuration.Models.DTOs.Generic
{
    public class TokenData
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
