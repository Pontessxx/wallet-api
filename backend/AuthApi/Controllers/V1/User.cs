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
    /// Atualiza o username de um usuário.
    /// </summary>
    /// <param name="request">Dados para atualização</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Usuário atualizado</returns>
    /// <response code="200">Usuário atualizado com sucesso</response>
    /// <response code="404">Usuário não encontrado</response>
    /// <response code="409">Username já existe</response>
    [HttpPut("edit")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Edit([FromBody] EditUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.UpdateAsync(request.Id, request.Username, ct);
            return Ok(new UserResponse(user.Id, user.Username));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza a senha do usuário autenticado.
    /// </summary>
    /// <param name="request">Senha atual e nova senha</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Senha atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Senha atual inválida ou usuário não autenticado</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpPut("edit-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditPassword([FromBody] EditPasswordRequest request, CancellationToken ct)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (string.IsNullOrWhiteSpace(request.CurrentPassword)
            || string.IsNullOrWhiteSpace(request.NewPassword)
            || request.NewPassword.Length < 8)
        {
            return BadRequest(new { message = "Dados inválidos. Informe senha atual e nova senha com no mínimo 8 caracteres." });
        }

        try
        {
            await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, ct);
            return Ok(new ChangePasswordResponse("Senha atualizada com sucesso."));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Senha atual inválida." });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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