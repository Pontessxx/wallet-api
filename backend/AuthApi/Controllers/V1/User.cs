using Application.Services;
using AuthApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace User.Controllers;

[ApiController]
[Route("user/v1")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retorna um usuário pelo ID.
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Usuário encontrado</returns>
    /// <response code="200">Usuário encontrado</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TODO
        return Ok();
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    /// <param name="request">Dados do usuário</param>
    /// <returns>Usuário criado</returns>
    /// <response code="201">Usuário criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="409">Usuário já existe</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request.Username, request.Password);
            var response = new UserResponse(user.Id, user.Username, user.Role.ToString());
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove um usuário pelo ID (soft delete).
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Removido com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpDelete("remove")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // TODO
        return NoContent();
    }
}