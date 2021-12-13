using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ChatApp.Domain.DbSet;

namespace ChatApp.DataService.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public virtual DbSet<User> Users { get; set; }
        //public virtual DbSet<Group> Groups { get; set; }
        //public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
    }
}
