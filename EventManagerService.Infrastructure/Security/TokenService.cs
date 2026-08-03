using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagerService.Infrastructure.DataAssets.Users;
using EventManagerService.Infrastructure.Security.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventManagerService.Infrastructure.Security
{
    /// <summary>
    /// Сервис для генерации подписанных JWT-токенов
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Генерирует подписанный JWT-токен для пользователя
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns>JWT-токен в виде строки</returns>
        public string GenerateToken(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.WriteToken(token);

                _logger.LogInformation($"JWT-токен успешно сгенерирован для пользователя {user.Login}");

                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при генерации JWT-токена: {ex.Message}");
                throw;
            }
        }
    }
}
