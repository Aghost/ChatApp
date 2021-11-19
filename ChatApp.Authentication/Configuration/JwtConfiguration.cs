using System;

namespace ChatApp.Authentication.Configuration
{
    public class JwtConfiguration
    {
        public string Secret { get; set; }
        public TimeSpan ExpiryTimeFrame { get; set; }
    }
}
