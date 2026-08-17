using System;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login);

        Task<User?> GetByIdAsync(Guid id);

        Task<User> CreateAsync(string login, string passwordHash, string roleName = "user");

        Task DeleteAsync(Guid id);

        Task<bool> ExistsByLoginAsync(string login);

        Task<bool> ExistsByIdAsync(Guid id);

        Task<bool> RoleExistsAsync(string roleName);
    }
}
