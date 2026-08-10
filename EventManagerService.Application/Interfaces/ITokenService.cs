namespace EventManagerService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с JWT-токенами
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Генерирует подписанный JWT-токен для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="login">Логин пользователя</param>
        /// <param name="role">Роль пользователя</param>
        /// <returns>JWT-токен в виде строки</returns>
        string GenerateToken(Guid userId, string login, string role);
    }
}
