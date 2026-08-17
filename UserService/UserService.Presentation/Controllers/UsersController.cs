using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Constants;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Presentation.Controllers;

/// <summary>
/// Контроллер для управления пользователями (создание, удаление)
/// </summary>
[ApiController]
[Route("users")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
    }

    /// <summary>
    /// Создаёт нового пользователя
    /// </summary>
    /// <param name="request">Данные для создания пользователя (логин, пароль и опциональная роль)</param>
    /// <returns>Данные созданного пользователя</returns>
    /// <remarks>
    /// POST /users
    ///
    /// Пример запроса:
    /// {
    ///   "login": "jane_doe",
    ///   "password": "securePassword123",
    ///   "role": "user"
    /// }
    ///
    /// Допустимые значения role: "user" (по умолчанию), "admin"
    /// Требуется роль "admin".
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        if (request == null)
            return BadRequest(new { message = "Request cannot be null" });

        try
        {
            var result = await _userManagementService.CreateUserAsync(request);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (AppException ex)
        {
            if (ex.ErrorCode == ErrorCodes.UserAlreadyExistsError)
                return Conflict(new { message = "User with this login already exists", errorCode = ex.ErrorCode });

            return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
        }
    }

    /// <summary>
    /// Удаляет пользователя по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <remarks>
    /// DELETE /users/{id}
    ///
    /// Требуется роль "admin".
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _userManagementService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (AppException ex)
        {
            if (ex.ErrorCode == ErrorCodes.NotFoundError)
                return NotFound(new { message = ex.Message, errorCode = ex.ErrorCode });

            return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
        }
    }
}
