using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Security.Configuration;

namespace UserService.Infrastructure.Security;

/// <summary>
/// Сервис для генерации подписанных JWT-токенов
/// Реализует интерфейс ITokenService из Application слоя
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
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="login">Логин пользователя</param>
    /// <param name="role">Роль пользователя</param>
    /// <returns>JWT-токен в виде строки</returns>
    public string GenerateToken(Guid userId, string login, string role)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, role ?? "User")
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

            _logger.LogInformation($"JWT-токен успешно сгенерирован для пользователя {login}");

            return jwt;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при генерации JWT-токена: {ex.Message}");
            throw;
        }
    }
}
