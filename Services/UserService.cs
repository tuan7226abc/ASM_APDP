using SIMS.DatabaseContext.Entities;
using SIMS.Interfaces;

namespace SIMS.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository repository)
        {
            userRepository = repository;
        }

        public async Task<User?> LoginUserAsync(
            string username,
            string password)
        {
            var user =
                await userRepository.GetUserByUsername(username);

            if (user == null)
            {
                return null;
            }

            if (user.Status != 1)
            {
                return null;
            }

            if (user.Password != password)
            {
                return null;
            }

            return user;
        }
    }
}