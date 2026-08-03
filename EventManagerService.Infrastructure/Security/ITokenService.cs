using EventManagerService.Infrastructure.DataAssets.Users;

namespace EventManagerService.Infrastructure.Security
{
    /// <summary>
    /// Сервис для генерации JWT-токенов
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Генерирует подписанный JWT-токен для пользователя
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns>JWT-токен в виде строки</returns>
        string GenerateToken(User user);
    }
}
