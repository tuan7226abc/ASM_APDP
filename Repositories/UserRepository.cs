using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SimsDbContext dbContext;
        public UserRepository(SimsDbContext context)
        {
            dbContext = context;
        }
        public async Task<User?> GetUserById(int id)
        {
            return await dbContext.Users.FindAsync(id).AsTask();
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
