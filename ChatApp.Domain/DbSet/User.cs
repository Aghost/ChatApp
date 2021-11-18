using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Domain.DbSet
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        //public List<Post> Posts { get; set; }
    }
}
