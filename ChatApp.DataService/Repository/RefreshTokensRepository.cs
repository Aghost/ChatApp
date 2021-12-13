using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChatApp.Domain.DbSet;
using ChatApp.DataService.IRepository;
using ChatApp.DataService.Data;

namespace ChatApp.DataService.Repository
{
    public class RefreshTokensRepository : GenericRepository<RefreshToken>, IRefreshTokensRepository
    {
        public RefreshTokensRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }
        
        public override async Task<IEnumerable<RefreshToken>> All()
        {
            try
            {
                return await _dbSet.Where(e => e.Status == 1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(RefreshTokensRepository)} All(get) method error");
                return new List<RefreshToken>();
            }
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                return _dbSet.Where(e => e.Token.ToLower() == refreshToken.ToLower())
                    .AsNoTracking()
                    .FirstOrDefault();
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(RefreshTokensRepository)} GetByRefreshToken method error");
                return new RefreshToken();
            }
        }

        public async Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken)
        {
            try
            {
                var token = _dbSet.Where(x => x.Token.ToLower() == refreshToken.Token.ToLower())
                    .AsNoTracking()
                    .FirstOrDefault();

                if (token == null) return false;

                token.IsUsed = refreshToken.IsUsed;
                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(RefreshTokensRepository)} GetByRefreshToken method error");
                return false;
            }
        }
    }
}
