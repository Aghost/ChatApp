using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.DataService.IRepository;
using ChatApp.Domain.DbSet;

namespace ChatApp.DataService.IConfiguration
{
    public interface IUserService
    {
        IUsersRepository Users { get; }
        IRefreshTokensRepository RefreshTokens { get; }
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById();
        Task<bool> AddUser();
        Task<bool> DeleteUser();
        Task<bool> CompleteAsync();
    }
}
