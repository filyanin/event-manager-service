using EventManagerService.Application.Interfaces;
using EventManagerService.Shared.Exceptions;

namespace EventManagerService.Application.Services
{
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
            _userRepository = userRepository ?? throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(passwordHasher));
            _tokenService = tokenService ?? throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(tokenService));
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
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(request));

            if (string.IsNullOrWhiteSpace(request.Login))
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, "Login is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, "Password is required");

            var roleName = string.IsNullOrWhiteSpace(request.Role) ? "user" : request.Role.ToLower();

            // Проверяем, что роль существует
            if (!await _userRepository.RoleExistsAsync(roleName))
            {
                var ex = new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed);
                ex.Data["role"] = roleName;
                throw ex;
            }

            // Проверяем, не существует ли пользователь с таким логином
            if (await _userRepository.ExistsByLoginAsync(request.Login))
            {
                var ex = new AppException(Shared.ErrorCodes.ErrorCodes.UserAlreadyExistsError);
                ex.Data["login"] = request.Login;
                throw ex;
            }

            // Хешируем пароль
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Создаем пользователя в базе данных
            var user = await _userRepository.CreateAsync(request.Login, passwordHash, roleName);

            // Генерируем токен
            var token = _tokenService.GenerateToken(user.Id, user.Login, user.RoleName);

            return new AuthenticationResponse
            {
                UserId = user.Id,
                Login = user.Login,
                Token = token
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
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, nameof(request));

            if (string.IsNullOrWhiteSpace(request.Login))
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, "Login is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new AppException(Shared.ErrorCodes.ErrorCodes.ValidationFailed, "Password is required");

            // Ищем пользователя по логину
            var user = await _userRepository.GetByLoginAsync(request.Login);

            // Проверяем, найден ли пользователь
            if (user == null)
            {
                throw new AppException(Shared.ErrorCodes.ErrorCodes.InvalidCredentialsError, "Invalid login or password");
            }

            // Проверяем пароль
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new AppException(Shared.ErrorCodes.ErrorCodes.InvalidCredentialsError, "Invalid login or password");
            }

            // Генерируем токен
            var token = _tokenService.GenerateToken(user.Id, user.Login, user.RoleName);

            return new AuthenticationResponse
            {
                UserId = user.Id,
                Login = user.Login,
                Token = token
            };
        }
    }
}
