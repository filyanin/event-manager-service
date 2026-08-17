using UserService.Application.Constants;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

/// <summary>
/// Сервис для управления пользователями (создание и удаление)
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new AppException(ErrorCodes.ValidationFailed, nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new AppException(ErrorCodes.ValidationFailed, nameof(passwordHasher));
    }

    /// <summary>
    /// Создаёт нового пользователя
    /// </summary>
    /// <param name="request">Данные для создания пользователя</param>
    /// <returns>Данные созданного пользователя</returns>
    /// <exception cref="AppException">Выбрасывается если пользователь уже существует, данные невалидны или роль не существует</exception>
    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (request == null)
            throw new AppException(ErrorCodes.ValidationFailed, nameof(request));

        if (string.IsNullOrWhiteSpace(request.Login))
            throw new AppException(ErrorCodes.ValidationFailed, "Login is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new AppException(ErrorCodes.ValidationFailed, "Password is required");

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "user" : request.Role.ToLower();

        if (!await _userRepository.RoleExistsAsync(roleName))
        {
            throw new AppException(ErrorCodes.ValidationFailed, $"Role '{roleName}' does not exist");
        }

        if (await _userRepository.ExistsByLoginAsync(request.Login))
        {
            throw new AppException(ErrorCodes.UserAlreadyExistsError, $"User with login '{request.Login}' already exists");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = await _userRepository.CreateAsync(request.Login, passwordHash, roleName);

        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role?.Name ?? roleName
        };
    }

    /// <summary>
    /// Удаляет пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <exception cref="AppException">Выбрасывается если пользователь не найден</exception>
    public async Task DeleteUserAsync(Guid userId)
    {
        if (!await _userRepository.ExistsByIdAsync(userId))
        {
            throw new AppException(ErrorCodes.NotFoundError, $"User with id '{userId}' not found");
        }

        await _userRepository.DeleteAsync(userId);
    }
}
