using System.Security.Cryptography;
using System.Text;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Хеширует пароль с использованием SHA-256
    /// </summary>
    /// <param name="password">Пароль для хеширования</param>
    /// <returns>Хеш пароля в формате Base64</returns>
    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Проверяет соответствие пароля его хешу
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <param name="hash">Хеш для сравнения</param>
    /// <returns>true если пароль соответствует хешу, иначе false</returns>
    public bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash, StringComparison.Ordinal);
    }
}
