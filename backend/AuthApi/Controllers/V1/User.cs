namespace User.Controllers;

[ApiController]
[Route("user/v1")]
[ApiExplorerSettings(GroupName = "v1")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retorna o usuário autenticado.
    /// </summary>
    /// <returns>Usuário encontrado</returns>
    /// <response code="200">Usuário encontrado</response>
    /// <response code="404">Usuário não encontrado</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var user = await _userService.GetByIdAsync(userId, ct);

        if (user is null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(new UserResponse(user.Id, user.Username));
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
    [Obsolete("Use /user/v2/create")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request.Username, request.Password);
            var response = new UserResponse(user.Id, user.Username);
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
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Removido com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpDelete("remove")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _userService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}