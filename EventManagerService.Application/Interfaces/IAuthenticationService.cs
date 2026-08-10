namespace EventManagerService.Application.Interfaces
{
    /// <summary>
    /// DTO для регистрации пользователя
    /// </summary>
    public class RegisterRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// Опциональное поле роли (по умолчанию "user")
        /// Допустимые значения: "user", "admin"
        /// </summary>
        public string? Role { get; set; } = "user";
    }

    /// <summary>
    /// DTO для входа пользователя
    /// </summary>
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// DTO для ответа после успешной аутентификации
    /// </summary>
    public class AuthenticationResponse
    {
        public Guid UserId { get; set; }
        public string Login { get; set; }
        public string Token { get; set; }
    }

    /// <summary>
    /// Сервис для аутентификации пользователей (регистрация и вход)
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="request">Данные для регистрации</param>
        /// <returns>Ответ с данными пользователя и токеном</returns>
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Осуществляет вход пользователя
        /// </summary>
        /// <param name="request">Данные для входа</param>
        /// <returns>Ответ с данными пользователя и токеном</returns>
        Task<AuthenticationResponse> LoginAsync(LoginRequest request);
    }
}
