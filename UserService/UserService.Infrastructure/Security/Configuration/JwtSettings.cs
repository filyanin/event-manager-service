namespace UserService.Infrastructure.Security.Configuration;

/// <summary>
/// Конфигурация для JWT токена
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Секретный ключ для подписи токена
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Издатель токена (Issuer)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Аудитория токена (Audience)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Время жизни токена в минутах
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
