using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserService.Application.Interfaces;
using UserService.Infrastructure.DataAssets;
using DomainEntities = UserService.Domain.Entities;
using DataUsers = UserService.Infrastructure.DataAssets.Users;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<DomainEntities.User?> GetByLoginAsync(string login)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == login);

            return user == null ? null : ToDomain(user);
        }

        public async Task<DomainEntities.User?> GetByIdAsync(Guid id)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : ToDomain(user);
        }

        public async Task<DomainEntities.User> CreateAsync(string login, string passwordHash, string roleName = "user")
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                throw new InvalidOperationException($"Role '{roleName}' not found in database");

            var newUser = new DataUsers.User
            {
                Id = Guid.NewGuid(),
                Login = login,
                PasswordHash = passwordHash,
                RoleId = role.Id
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return ToDomain(newUser, role);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsByLoginAsync(string login)
        {
            return await _dbContext.Users.AnyAsync(u => u.Login == login);
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbContext.Roles.AnyAsync(r => r.Name == roleName);
        }

        private static DomainEntities.User ToDomain(DataUsers.User user, DataUsers.Role? role = null)
        {
            var effectiveRole = user.Role ?? role;

            return new DomainEntities.User
            {
                Id = user.Id,
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                RoleId = user.RoleId,
                Role = effectiveRole == null
                    ? null
                    : new DomainEntities.Role
                    {
                        Id = effectiveRole.Id,
                        Name = effectiveRole.Name,
                        IsAdmin = effectiveRole.IsAdmin
                    }
            };
        }
    }
}
