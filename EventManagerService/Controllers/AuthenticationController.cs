using EventManagerService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerService.Controllers
{
    /// <summary>
    /// Контроллер для управления аутентификацией пользователей
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="request">Данные для регистрации (логин, пароль и опциональная роль)</param>
        /// <returns>Данные пользователя и JWT-токен</returns>
        /// <remarks>
        /// POST /auth/register
        /// 
        /// Пример запроса:
        /// {
        ///   "login": "john_doe",
        ///   "password": "securePassword123",
        ///   "role": "user"
        /// }
        /// 
        /// Допустимые значения role: "user" (по умолчанию), "admin"
        /// Доступно без аутентификации.
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request cannot be null" });

            try
            {
                var result = await _authenticationService.RegisterAsync(request);
                return Ok(result);
            }
            catch (Shared.Exceptions.AppException ex)
            {
                if (ex.ErrorCode == Shared.ErrorCodes.ErrorCodes.UserAlreadyExistsError)
                    return Conflict(new { message = "User with this login already exists", errorCode = ex.ErrorCode });

                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
        }

        /// <summary>
        /// Осуществляет вход пользователя
        /// </summary>
        /// <param name="request">Данные для входа (логин и пароль)</param>
        /// <returns>Данные пользователя и JWT-токен</returns>
        /// <remarks>
        /// POST /auth/login
        /// 
        /// Пример запроса:
        /// {
        ///   "login": "john_doe",
        ///   "password": "securePassword123"
        /// }
        /// 
        /// Доступно без аутентификации.
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request cannot be null" });

            try
            {
                var result = await _authenticationService.LoginAsync(request);
                return Ok(result);
            }
            catch (Shared.Exceptions.AppException ex)
            {
                if (ex.ErrorCode == Shared.ErrorCodes.ErrorCodes.InvalidCredentialsError)
                    return Unauthorized(new { message = "Invalid login or password", errorCode = ex.ErrorCode });

                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
        }
    }
}
