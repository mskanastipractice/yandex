using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Auth;

namespace WebAPI.Controllers;

/// <summary>
/// Представляет контроллер аутентификации.
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService): ControllerBase
{
    /// <summary>
    /// Регистрирует пользователя в системе.
    /// </summary>
    /// <param name="data">Данные для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Register([FromBody] RegistrationRequest data, CancellationToken cancellationToken)
    {
        await authService.RegisterAsync(data.ToDto(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Осуществляет вход пользователя в систему.
    /// </summary>
    /// <param name="data">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest data, CancellationToken cancellationToken)
    {
        var token = await authService.LoginAsync(data.ToDto(), cancellationToken);
        return Ok(token);
    }
}