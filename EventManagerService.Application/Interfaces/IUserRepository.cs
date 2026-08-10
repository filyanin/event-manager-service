namespace EventManagerService.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Получает пользователя по логину
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <returns>Пользователь или null если не найден</returns>
        Task<UserData?> GetByLoginAsync(string login);

        /// <summary>
        /// Создает нового пользователя с указанной ролью
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <param name="roleName">Имя роли (по умолчанию "user")</param>
        /// <returns>Созданный пользователь с указанными данными</returns>
        Task<UserData> CreateAsync(string login, string passwordHash, string roleName = "user");

        /// <summary>
        /// Проверяет существует ли пользователь с данным логином
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <returns>true если пользователь существует, иначе false</returns>
        Task<bool> ExistsByLoginAsync(string login);

        /// <summary>
        /// Проверяет существует ли роль с данным именем
        /// </summary>
        /// <param name="roleName">Имя роли</param>
        /// <returns>true если роль существует, иначе false</returns>
        Task<bool> RoleExistsAsync(string roleName);
    }

    /// <summary>
    /// DTO для передачи данных пользователя между слоями
    /// </summary>
    public class UserData
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string RoleName { get; set; }
    }
}
