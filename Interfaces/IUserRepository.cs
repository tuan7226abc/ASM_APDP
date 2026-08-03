using SIMS.DatabaseContext.Entities;

namespace SIMS.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserById(int id);
    }
}
