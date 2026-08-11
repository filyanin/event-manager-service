using System.Security.Cryptography;
using System.Text;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Security;

/// <summary>
/// Хеширование паролей с использованием PBKDF2 (RFC 2898) со случайной солью
/// и constant-time сравнением для защиты от timing-атак
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 210_000; // OWASP-рекомендация для PBKDF2-HMAC-SHA256 (2023+)
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Хеширует пароль с использованием PBKDF2 (соль + большое число итераций)
    /// </summary>
    /// <param name="password">Пароль для хеширования</param>
    /// <returns>Строка вида {итерации}.{соль}.{хеш} в формате Base64</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            HashSizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Проверяет соответствие пароля его хешу
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <param name="hash">Хеш для сравнения (формат {итерации}.{соль}.{хеш})</param>
    /// <returns>true если пароль соответствует хешу, иначе false</returns>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        var parts = hash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt, expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedHash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            Algorithm,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
