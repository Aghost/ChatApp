using System;

namespace ChatApp.Domain.DbSet
{
    public class User : BaseEntity
    {
        public Guid IdentityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        //public List<Post> Posts { get; set; }
    }
}
