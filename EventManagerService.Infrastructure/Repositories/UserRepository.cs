using EventManagerService.Application.Interfaces;
using EventManagerService.Infrastructure.DataAssets;
using EventManagerService.Infrastructure.DataAssets.Users;
using Microsoft.EntityFrameworkCore;

namespace EventManagerService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Получает пользователя по логину
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <returns>Пользователь или null если не найден</returns>
        public async Task<UserData?> GetByLoginAsync(string login)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                return null;

            return new UserData
            {
                Id = user.Id,
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                RoleName = user.Role?.Name ?? "user"
            };
        }

        /// <summary>
        /// Создает нового пользователя с указанной ролью
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <param name="roleName">Имя роли (по умолчанию "user")</param>
        /// <returns>Созданный пользователь с указанными данными</returns>
        public async Task<UserData> CreateAsync(string login, string passwordHash, string roleName = "user")
        {
            // Получаем роль пользователя
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                throw new InvalidOperationException($"Role '{roleName}' not found in database");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                PasswordHash = passwordHash,
                Role = role
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return new UserData
            {
                Id = newUser.Id,
                Login = newUser.Login,
                PasswordHash = newUser.PasswordHash,
                RoleName = newUser.Role.Name
            };
        }

        /// <summary>
        /// Проверяет существует ли пользователь с данным логином
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <returns>true если пользователь существует, иначе false</returns>
        public async Task<bool> ExistsByLoginAsync(string login)
        {
            return await _dbContext.Users.AnyAsync(u => u.Login == login);
        }

        /// <summary>
        /// Проверяет существует ли роль с данным именем
        /// </summary>
        /// <param name="roleName">Имя роли</param>
        /// <returns>true если роль существует, иначе false</returns>
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbContext.Roles.AnyAsync(r => r.Name == roleName);
        }
    }
}
