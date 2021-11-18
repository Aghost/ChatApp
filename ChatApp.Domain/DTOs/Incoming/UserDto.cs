using System;

namespace ChatApp.Domain.DTOs.Incoming
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        //public List<Post> Posts { get; set; }
    }
}
