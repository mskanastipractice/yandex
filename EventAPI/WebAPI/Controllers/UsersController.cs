using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Mappings;
using WebAPI.Models.Users;

namespace WebAPI.Controllers;

/// <summary>
/// Представляет контроллер для пользователей.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userDto = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(userDto.ToResponse());
    }
}