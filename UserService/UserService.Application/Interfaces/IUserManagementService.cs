namespace UserService.Application.Interfaces;

/// <summary>
/// DTO для создания пользователя администратором
/// </summary>
public class CreateUserRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Опциональное поле роли (по умолчанию "user")
    /// Допустимые значения: "user", "admin"
    /// </summary>
    public string? Role { get; set; } = "user";
}

/// <summary>
/// DTO с данными пользователя
/// </summary>
public class UserResponse
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Сервис для управления пользователями (создание, удаление)
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Создаёт нового пользователя
    /// </summary>
    /// <param name="request">Данные для создания пользователя</param>
    /// <returns>Данные созданного пользователя</returns>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Удаляет пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    Task DeleteUserAsync(Guid userId);
}
