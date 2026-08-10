using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserService.Application.Interfaces;
using UserService.Infrastructure.DataAssets;
using UserService.Infrastructure.DataAssets.Users;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<UserData?> GetByLoginAsync(string login)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                return null;

            var roleName = await _dbContext.Roles
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            return new UserData
            {
                Id = user.Id,
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                RoleName = roleName ?? "user"
            };
        }

        public async Task<UserData> CreateAsync(string login, string passwordHash, string roleName = "user")
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                throw new InvalidOperationException($"Role '{roleName}' not found in database");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                PasswordHash = passwordHash,
                RoleId = role.Id
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return new UserData
            {
                Id = newUser.Id,
                Login = newUser.Login,
                PasswordHash = newUser.PasswordHash,
                RoleName = role.Name
            };
        }

        public async Task<bool> ExistsByLoginAsync(string login)
        {
            return await _dbContext.Users.AnyAsync(u => u.Login == login);
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbContext.Roles.AnyAsync(r => r.Name == roleName);
        }
    }
}
