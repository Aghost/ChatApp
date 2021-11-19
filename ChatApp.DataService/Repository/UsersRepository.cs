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
    public class UsersRepository : GenericRepository<User>, IUsersRepository
    {
        public UsersRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }
        
        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await _dbSet.Where(e => e.Status == 1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(UsersRepository)} All(get) method error");
                return new List<User>();
            }
        }

        public override async Task<bool> Upsert(User entity)
        {
            try
            {
                var existingUser = await _dbSet.Where(x => x.Id == entity.Id).FirstOrDefaultAsync();

                if (existingUser == null) { return await Add(entity); }

                existingUser.Status = entity.Status;
                existingUser.UserName = entity.UserName;
                existingUser.FirstName = entity.FirstName;
                existingUser.LastName = entity.LastName;
                existingUser.Email = entity.Email;

                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(UsersRepository)} Upsert method error");

                return false;
            }
        }

        public override async Task<bool> Delete(Guid id, string userId)
        {
            try
            {
                var exists = await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (exists != null)
                {
                    _dbSet.Remove(exists);
                    return true;
                }

                return false;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(UsersRepository)} Delete method error");

                return false;
            }
        }

    }
}
