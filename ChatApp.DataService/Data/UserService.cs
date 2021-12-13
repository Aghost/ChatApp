using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatApp.DataService.IConfiguration;
using ChatApp.DataService.IRepository;
using ChatApp.DataService.Repository;
using ChatApp.Domain.DbSet;
using System.Collections.Generic;

namespace ChatApp.DataService.Data
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;
        
        public IUsersRepository Users { get; private set; }
        public IRefreshTokensRepository RefreshTokens { get; private set; }

        public UserService(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("db_logs");

            Users = new UsersRepository(context, _logger);
            RefreshTokens = new RefreshTokensRepository(context, _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<IEnumerable<User>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserById()
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddUser()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUser()
        {
            throw new NotImplementedException();
        }
    }
}
