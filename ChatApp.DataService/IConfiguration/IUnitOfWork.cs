using System.Threading.Tasks;
using ChatApp.DataService.IRepository;

namespace ChatApp.DataService.IConfiguration
{
    public interface IUnitOfWork
    {
        IUsersRepository Users { get; }
        IRefreshTokensRepository RefreshTokens { get; }
        Task CompleteAsync();
    }
}
