using System;
using System.Threading.Tasks;

namespace UserService.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public interface IUserRepository
    {
        Task<UserData?> GetByLoginAsync(string login);

        Task<UserData> CreateAsync(string login, string passwordHash, string roleName = "user");

        Task<bool> ExistsByLoginAsync(string login);

        Task<bool> RoleExistsAsync(string roleName);
    }

    public class UserData
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string RoleName { get; set; }
    }
}
