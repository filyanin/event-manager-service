namespace UserService.Application.Interfaces;

public interface IPasswordHasher
{
    /// <summary>
    /// Хеширует пароль с использованием SHA-256
    /// </summary>
    /// <param name="password">Пароль для хеширования</param>
    /// <returns>Хеш пароля в формате Base64</returns>
    string HashPassword(string password);

    /// <summary>
    /// Проверяет соответствие пароля его хешу
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <param name="hash">Хеш для сравнения</param>
    /// <returns>true если пароль соответствует хешу, иначе false</returns>
    bool VerifyPassword(string password, string hash);
}
