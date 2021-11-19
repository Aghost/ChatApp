using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.DbSet;

namespace ChatApp.DataService.IRepository
{
    public interface IRefreshTokensRepository : IGenericRepository<RefreshToken>
    {
    }
}
