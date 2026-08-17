using UserService.Application.Constants;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

/// <summary>
/// Сервис для аутентификации пользователей (регистрация и вход)
/// Обеспечивает логику регистрации с хешированием пароля и входа с проверкой учетных данных
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository ?? throw new AppException(ErrorCodes.ValidationFailed, nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new AppException(ErrorCodes.ValidationFailed, nameof(passwordHasher));
        _tokenService = tokenService ?? throw new AppException(ErrorCodes.ValidationFailed, nameof(tokenService));
    }

    /// <summary>
    /// Регистрирует нового пользователя с хешированным паролем
    /// </summary>
    /// <param name="request">Данные для регистрации (логин, пароль и опциональная роль)</param>
    /// <returns>Ответ с данными пользователя и JWT-токеном</returns>
    /// <exception cref="AppException">Выбрасывается если пользователь уже существует, данные невалидны или роль не существует</exception>
    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
            throw new AppException(ErrorCodes.ValidationFailed, nameof(request));

        if (string.IsNullOrWhiteSpace(request.Login))
            throw new AppException(ErrorCodes.ValidationFailed, "Login is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new AppException(ErrorCodes.ValidationFailed, "Password is required");

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "user" : request.Role.ToLower();

        // Проверяем, что роль существует
        if (!await _userRepository.RoleExistsAsync(roleName))
        {
            throw new AppException(ErrorCodes.ValidationFailed, $"Role '{roleName}' does not exist");
        }

        // Проверяем, не существует ли пользователь с таким логином
        if (await _userRepository.ExistsByLoginAsync(request.Login))
        {
            throw new AppException(ErrorCodes.UserAlreadyExistsError, $"User with login '{request.Login}' already exists");
        }

        // Хешируем пароль
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Создаем пользователя в базе данных
        var user = await _userRepository.CreateAsync(request.Login, passwordHash, roleName);

        // Генерируем токен
        var userRoleName = user.Role?.Name ?? roleName;
        var token = _tokenService.GenerateToken(user.Id, user.Login, userRoleName);

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Token = token,
            Role = userRoleName
        };
    }

    /// <summary>
    /// Осуществляет вход пользователя по логину и паролю
    /// </summary>
    /// <param name="request">Данные для входа (логин и пароль)</param>
    /// <returns>Ответ с данными пользователя и JWT-токеном</returns>
    /// <exception cref="AppException">Выбрасывается если учетные данные неверны</exception>
    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        if (request == null)
            throw new AppException(ErrorCodes.ValidationFailed, nameof(request));

        if (string.IsNullOrWhiteSpace(request.Login))
            throw new AppException(ErrorCodes.ValidationFailed, "Login is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new AppException(ErrorCodes.ValidationFailed, "Password is required");

        // Ищем пользователя по логину
        var user = await _userRepository.GetByLoginAsync(request.Login);

        // Проверяем, найден ли пользователь
        if (user == null)
        {
            throw new AppException(ErrorCodes.InvalidCredentialsError, "Invalid login or password");
        }

        // Проверяем пароль
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AppException(ErrorCodes.InvalidCredentialsError, "Invalid login or password");
        }

        // Генерируем токен
        var userRoleName = user.Role?.Name ?? "user";
        var token = _tokenService.GenerateToken(user.Id, user.Login, userRoleName);

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Token = token,
            Role = userRoleName
        };
    }
}
